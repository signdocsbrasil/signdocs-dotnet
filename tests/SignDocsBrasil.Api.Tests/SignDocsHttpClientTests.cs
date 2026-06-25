using System.Net;
using System.Text;
using System.Text.Json;
using SignDocsBrasil.Api.Errors;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class SignDocsHttpClientTests : IDisposable
{
    private readonly SignDocsHttpClient _client;
    private readonly MockHttpHandler _handler;

    public SignDocsHttpClientTests()
    {
        (_client, _handler) = TestClientFactory.Create();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task Get_ReturnsDeserializedResponse()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"status":"healthy","version":"1.0.0"}""");

        var result = await _client.RequestAsync<JsonElement>(
            HttpMethod.Get, "/health");

        Assert.Equal("healthy", result.GetProperty("status").GetString());
        Assert.Equal("1.0.0", result.GetProperty("version").GetString());
    }

    [Fact]
    public async Task Get_SetsAuthorizationHeader()
    {
        _handler.EnqueueToken("my-bearer-token");
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        // Requests: [0]=token, [1]=API call
        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal("Bearer my-bearer-token",
            apiRequest.Headers.GetValues("Authorization").First());
    }

    [Fact]
    public async Task Get_SetsUserAgentHeader()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal(SignDocsHttpClient.UserAgent,
            apiRequest.Headers.GetValues("User-Agent").First());
    }

    [Fact]
    public async Task Post_SendsJsonBody()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(201, """{"id":"created"}""");

        var body = new { name = "test", value = 42 };
        await _client.RequestAsync<JsonElement>(
            HttpMethod.Post, "/v1/items", body: body);

        string content = _handler.RequestBodies[1]!;
        using var doc = JsonDocument.Parse(content);
        Assert.Equal("test", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal(42, doc.RootElement.GetProperty("value").GetInt32());
    }

    [Fact]
    public async Task Post_WithNullBody_SendsEmptyJsonObject()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(
            HttpMethod.Post, "/v1/action", body: null);

        string content = _handler.RequestBodies[1]!;
        Assert.Equal("{}", content);
    }

    [Fact]
    public async Task Put_WithNullBody_SendsEmptyJsonObject()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(
            HttpMethod.Put, "/v1/resource", body: null);

        string content = _handler.RequestBodies[1]!;
        Assert.Equal("{}", content);
    }

    [Fact]
    public async Task Delete_WithNoBody_HasNoContent()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(
            HttpMethod.Delete, "/v1/resource/123");

        Assert.Null(_handler.RequestBodies[1]);
    }

    [Fact]
    public async Task Response204_ReturnsDefault()
    {
        _handler.EnqueueToken();
        _handler.EnqueueNoContent();

        var result = await _client.RequestAsync<JsonElement>(
            HttpMethod.Delete, "/v1/webhooks/wh-001");

        Assert.Equal(default, result);
    }

    [Fact]
    public async Task CustomHeaders_ArePassedThrough()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        var headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "custom-value" },
            { "X-Idempotency-Key", "idem-123" }
        };

        await _client.RequestAsync<JsonElement>(
            HttpMethod.Post, "/v1/test", body: new { }, headers: headers);

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal("custom-value",
            apiRequest.Headers.GetValues("X-Custom-Header").First());
        Assert.Equal("idem-123",
            apiRequest.Headers.GetValues("X-Idempotency-Key").First());
    }

    [Fact]
    public async Task NoAuth_SkipsAuthorizationHeader()
    {
        _handler.EnqueueJson(200, """{"status":"healthy"}""");

        await _client.RequestNoAuthAsync<JsonElement>(HttpMethod.Get, "/health");

        // Only one request (no token request)
        Assert.Single(_handler.Requests);
        HttpRequestMessage req = _handler.Requests[0];
        Assert.False(req.Headers.Contains("Authorization"));
    }

    [Fact]
    public async Task QueryParameters_AreBuiltCorrectly()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"items":[]}""");

        var query = new Dictionary<string, string>
        {
            { "status", "COMPLETED" },
            { "limit", "10" }
        };

        await _client.RequestAsync<JsonElement>(
            HttpMethod.Get, "/v1/transactions", query: query);

        HttpRequestMessage apiRequest = _handler.Requests[1];
        string url = apiRequest.RequestUri!.ToString();
        Assert.Contains("status=COMPLETED", url);
        Assert.Contains("limit=10", url);
    }

    [Fact]
    public async Task QueryParameters_UrlEncodesValues()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"items":[]}""");

        var query = new Dictionary<string, string>
        {
            { "filter", "a&b=c" }
        };

        await _client.RequestAsync<JsonElement>(
            HttpMethod.Get, "/v1/test", query: query);

        HttpRequestMessage apiRequest = _handler.Requests[1];
        string url = apiRequest.RequestUri!.ToString();
        Assert.Contains("filter=a%26b%3Dc", url);
    }

    [Fact]
    public async Task Error400_ThrowsBadRequestException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(400,
            """{"type":"https://api.signdocs.com.br/errors/bad-request","title":"Bad Request","status":400,"detail":"Invalid input"}""");

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));

        Assert.Equal(400, ex.Status);
        Assert.Equal("Invalid input", ex.Detail);
    }

    [Fact]
    public async Task Error401_ThrowsUnauthorizedException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(401,
            """{"type":"test","title":"Unauthorized","status":401,"detail":"Invalid token"}""");

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task Error403_ThrowsForbiddenException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(403,
            """{"type":"test","title":"Forbidden","status":403,"detail":"Access denied"}""");

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task Error404_ThrowsNotFoundException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(404,
            """{"type":"test","title":"Not Found","status":404,"detail":"Not found"}""");

        await Assert.ThrowsAsync<NotFoundException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task Error409_ThrowsConflictException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(409,
            """{"type":"test","title":"Conflict","status":409,"detail":"Already exists"}""");

        await Assert.ThrowsAsync<ConflictException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task Error422_ThrowsUnprocessableEntityException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(422,
            """{"type":"test","title":"Unprocessable","status":422,"detail":"Validation failed"}""");

        await Assert.ThrowsAsync<UnprocessableEntityException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task Error429_ThrowsRateLimitException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(429,
            """{"type":"test","title":"Rate Limit","status":429,"detail":"Too many requests"}""");

        await Assert.ThrowsAsync<RateLimitException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task Error500_ThrowsInternalServerException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(500,
            """{"type":"test","title":"Server Error","status":500,"detail":"Unexpected"}""");

        await Assert.ThrowsAsync<InternalServerException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task Error503_ThrowsServiceUnavailableException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(503,
            """{"type":"test","title":"Unavailable","status":503,"detail":"Maintenance"}""");

        await Assert.ThrowsAsync<ServiceUnavailableException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task UnknownError_ThrowsApiException()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(418,
            """{"type":"test","title":"Teapot","status":418,"detail":"I'm a teapot"}""");

        var ex = await Assert.ThrowsAsync<ApiException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
        Assert.Equal(418, ex.Status);
    }

    [Fact]
    public async Task ErrorWithoutProblemJsonType_UsesFallback()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(400, """{"message":"plain error"}""");

        var ex = await Assert.ThrowsAsync<BadRequestException>(
            () => _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));

        Assert.Contains("400", ex.ProblemDetail.Type!);
    }

    [Fact]
    public async Task RequestWithIdempotency_SetsIdempotencyKey()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(201, """{"id":"created"}""");

        await _client.RequestWithIdempotencyAsync<JsonElement>(
            HttpMethod.Post, "/v1/webhooks",
            body: new { url = "https://example.com" },
            idempotencyKey: "my-key-123");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal("my-key-123",
            apiRequest.Headers.GetValues("X-Idempotency-Key").First());
    }

    [Fact]
    public async Task RequestWithIdempotency_GeneratesKeyWhenNull()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(201, """{"id":"created"}""");

        await _client.RequestWithIdempotencyAsync<JsonElement>(
            HttpMethod.Post, "/v1/webhooks",
            body: new { url = "https://example.com" });

        HttpRequestMessage apiRequest = _handler.Requests[1];
        string key = apiRequest.Headers.GetValues("X-Idempotency-Key").First();
        Assert.NotNull(key);
        Assert.True(Guid.TryParse(key, out _));
    }

    [Fact]
    public void BuildUrl_WithQueryParams()
    {
        string url = _client.BuildUrl("/v1/transactions", new Dictionary<string, string>
        {
            { "status", "CREATED" },
            { "limit", "5" }
        });

        Assert.Equal("https://api.test.com/v1/transactions?status=CREATED&limit=5", url);
    }

    [Fact]
    public void BuildUrl_WithoutQueryParams()
    {
        string url = _client.BuildUrl("/v1/transactions/tx-001", null);
        Assert.Equal("https://api.test.com/v1/transactions/tx-001", url);
    }

    [Fact]
    public void BuildUrl_WithEmptyQuery()
    {
        string url = _client.BuildUrl("/v1/test", new Dictionary<string, string>());
        Assert.Equal("https://api.test.com/v1/test", url);
    }

    [Fact]
    public void BuildUrl_EncodesSpecialCharacters()
    {
        string url = _client.BuildUrl("/v1/test", new Dictionary<string, string>
        {
            { "q", "hello world" },
            { "tag", "a&b" }
        });

        Assert.Contains("q=hello%20world", url);
        Assert.Contains("tag=a%26b", url);
    }

    [Fact]
    public void BuildUrl_SkipsNullValues()
    {
        string url = _client.BuildUrl("/v1/test", new Dictionary<string, string>
        {
            { "status", "CREATED" },
            { "filter", null! }
        });

        Assert.Contains("status=CREATED", url);
        Assert.DoesNotContain("filter", url);
    }

    [Fact]
    public void SdkVersion_Is1_6_0()
    {
        Assert.Equal("1.6.0", SignDocsHttpClient.SdkVersion);
    }

    [Fact]
    public void UserAgent_ContainsVersion()
    {
        Assert.Equal("signdocs-brasil-dotnet/1.6.0", SignDocsHttpClient.UserAgent);
    }

    [Fact]
    public void JsonOptions_UsesCamelCase()
    {
        Assert.Equal(JsonNamingPolicy.CamelCase, SignDocsHttpClient.JsonOptions.PropertyNamingPolicy);
    }
}
