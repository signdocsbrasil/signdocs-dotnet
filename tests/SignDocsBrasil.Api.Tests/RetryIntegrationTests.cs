using System.Net;
using System.Text;
using System.Text.Json;
using SignDocsBrasil.Api.Errors;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class RetryIntegrationTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
    {
        foreach (var d in _disposables) d.Dispose();
    }

    [Fact]
    public async Task RetriesOn429_ThenSucceeds()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 3);
        _disposables.Add(client);

        handler.EnqueueToken();

        // First attempt: 429
        var retryResponse = new HttpResponseMessage((HttpStatusCode)429)
        {
            Content = new StringContent(
                """{"type":"test","title":"Rate Limit","status":429,"detail":"Too many"}""",
                Encoding.UTF8, "application/problem+json")
        };
        retryResponse.Headers.TryAddWithoutValidation("Retry-After", "0");
        handler.Enqueue(retryResponse);

        // Second attempt: success
        handler.EnqueueJson(200, """{"status":"ok"}""");

        var result = await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        Assert.Equal("ok", result.GetProperty("status").GetString());
        // token + 429 + 200 = 3 requests
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task RetriesOn500_ThenSucceeds()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 2);
        _disposables.Add(client);

        handler.EnqueueToken();

        // First attempt: 500
        handler.EnqueueProblemJson(500,
            """{"type":"test","title":"Error","status":500,"detail":"Server error"}""");

        // Second attempt: success
        handler.EnqueueJson(200, """{"result":"success"}""");

        var result = await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        Assert.Equal("success", result.GetProperty("result").GetString());
    }

    [Fact]
    public async Task RetriesOn503_ThenSucceeds()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 2);
        _disposables.Add(client);

        handler.EnqueueToken();

        // First attempt: 503
        handler.EnqueueProblemJson(503,
            """{"type":"test","title":"Unavailable","status":503,"detail":"Maintenance"}""");

        // Second attempt: success
        handler.EnqueueJson(200, """{"available":true}""");

        var result = await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        Assert.True(result.GetProperty("available").GetBoolean());
    }

    [Fact]
    public async Task GivesUpAfterMaxRetries()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 2);
        _disposables.Add(client);

        handler.EnqueueToken();

        // All 3 attempts (1 initial + 2 retries) return 500
        for (int i = 0; i < 3; i++)
        {
            handler.EnqueueProblemJson(500,
                """{"type":"test","title":"Error","status":500,"detail":"Persistent error"}""");
        }

        await Assert.ThrowsAsync<InternalServerException>(
            () => client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task GivesUpAfterMaxRetries_503()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 1);
        _disposables.Add(client);

        handler.EnqueueToken();

        // 2 attempts (1 initial + 1 retry) return 503
        handler.EnqueueProblemJson(503,
            """{"type":"test","title":"Unavailable","status":503,"detail":"Down"}""");
        handler.EnqueueProblemJson(503,
            """{"type":"test","title":"Unavailable","status":503,"detail":"Still down"}""");

        await Assert.ThrowsAsync<ServiceUnavailableException>(
            () => client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));
    }

    [Fact]
    public async Task DoesNotRetry400()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 3);
        _disposables.Add(client);

        handler.EnqueueToken();
        handler.EnqueueProblemJson(400,
            """{"type":"test","title":"Bad Request","status":400,"detail":"Invalid"}""");

        await Assert.ThrowsAsync<BadRequestException>(
            () => client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));

        // token + single 400 = 2 requests (no retry)
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task DoesNotRetry404()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 3);
        _disposables.Add(client);

        handler.EnqueueToken();
        handler.EnqueueProblemJson(404,
            """{"type":"test","title":"Not Found","status":404,"detail":"Missing"}""");

        await Assert.ThrowsAsync<NotFoundException>(
            () => client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));

        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task DoesNotRetry401()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 3);
        _disposables.Add(client);

        handler.EnqueueToken();
        handler.EnqueueProblemJson(401,
            """{"type":"test","title":"Unauthorized","status":401,"detail":"Bad token"}""");

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));

        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task MultipleRetries_ThenSucceeds()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 5);
        _disposables.Add(client);

        handler.EnqueueToken();

        // 3 failures then success
        for (int i = 0; i < 3; i++)
        {
            handler.EnqueueProblemJson(500,
                """{"type":"test","title":"Error","status":500,"detail":"Transient"}""");
        }
        handler.EnqueueJson(200, """{"recovered":true}""");

        var result = await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        Assert.True(result.GetProperty("recovered").GetBoolean());
        // token + 3 failures + 1 success = 5 total
        Assert.Equal(5, handler.Requests.Count);
    }

    [Fact]
    public async Task NoRetries_WhenMaxRetriesIsZero()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 0);
        _disposables.Add(client);

        handler.EnqueueToken();
        handler.EnqueueProblemJson(500,
            """{"type":"test","title":"Error","status":500,"detail":"No retries"}""");

        await Assert.ThrowsAsync<InternalServerException>(
            () => client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test"));

        // token + single 500 = 2 requests
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task RespectsRetryAfterHeader()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 1);
        _disposables.Add(client);

        handler.EnqueueToken();

        // 429 with Retry-After: 0 (to keep tests fast)
        var response429 = new HttpResponseMessage((HttpStatusCode)429)
        {
            Content = new StringContent(
                """{"type":"test","title":"Rate Limit","status":429,"detail":"Slow down"}""",
                Encoding.UTF8, "application/problem+json")
        };
        response429.Headers.TryAddWithoutValidation("Retry-After", "0");
        handler.Enqueue(response429);

        handler.EnqueueJson(200, """{"ok":true}""");

        var result = await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        Assert.True(result.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public async Task RetryPreservesOriginalRequest()
    {
        var (client, handler) = TestClientFactory.Create(maxRetries: 1);
        _disposables.Add(client);

        handler.EnqueueToken();
        handler.EnqueueProblemJson(500,
            """{"type":"test","title":"Error","status":500,"detail":"Retry me"}""");
        handler.EnqueueJson(200, """{"result":"ok"}""");

        await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/transactions/tx-001");

        // Both API requests should be to the same path
        // Requests: [0]=token, [1]=first attempt, [2]=retry
        Assert.Equal("/v1/transactions/tx-001", handler.Requests[1].RequestUri!.AbsolutePath);
        Assert.Equal("/v1/transactions/tx-001", handler.Requests[2].RequestUri!.AbsolutePath);
    }
}
