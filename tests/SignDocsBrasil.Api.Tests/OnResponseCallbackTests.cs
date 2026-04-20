using System.Net;
using System.Text.Json;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class OnResponseCallbackTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
    {
        foreach (IDisposable d in _disposables) d.Dispose();
    }

    [Fact]
    public async Task OnResponse_IsInvokedAfterEachRequest()
    {
        var captured = new List<ResponseMetadata>();
        var (client, handler) = CreateClient(md => captured.Add(md));
        _disposables.Add(client);

        handler.EnqueueToken();
        handler.EnqueueJson(200, """{"ok":true}""");

        await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        // OnResponse observes API calls only, not OAuth token fetches.
        // Token endpoint has its own error path; observability is for
        // business endpoints. Matches the PHP / TS / Python / Java /
        // Go behavior.
        Assert.Single(captured);
        Assert.Equal(200, captured[0].StatusCode);
        Assert.Equal("GET", captured[0].Method);
        Assert.Equal("/v1/test", captured[0].Path);
    }

    [Fact]
    public async Task OnResponse_ReceivesRateLimitHeaders()
    {
        var captured = new List<ResponseMetadata>();
        var (client, handler) = CreateClient(md => captured.Add(md));
        _disposables.Add(client);

        handler.EnqueueToken();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"ok":true}""", System.Text.Encoding.UTF8, "application/json")
        };
        response.Headers.Add("RateLimit-Limit", "100");
        response.Headers.Add("RateLimit-Remaining", "42");
        response.Headers.Add("X-Request-Id", "abc-123");
        handler.Enqueue(response);

        await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        ResponseMetadata apiCall = captured[^1];
        Assert.Equal(100, apiCall.RateLimitLimit);
        Assert.Equal(42, apiCall.RateLimitRemaining);
        Assert.Equal("abc-123", apiCall.RequestId);
    }

    [Fact]
    public async Task OnResponse_CallbackException_DoesNotBreakRequest()
    {
        var (client, handler) = CreateClient(_ => throw new InvalidOperationException("boom"));
        _disposables.Add(client);

        handler.EnqueueToken();
        handler.EnqueueJson(200, """{"ok":true}""");

        // Should not throw despite the callback throwing
        var result = await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        Assert.Equal("true", result.GetProperty("ok").GetRawText());
    }

    [Fact]
    public async Task OnResponse_IsOptional()
    {
        var (client, handler) = CreateClient(onResponse: null);
        _disposables.Add(client);

        handler.EnqueueToken();
        handler.EnqueueJson(200, """{"ok":true}""");

        // Should not throw when no callback is registered
        var result = await client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        Assert.Equal("true", result.GetProperty("ok").GetRawText());
    }

    private static (SignDocsHttpClient client, MockHttpHandler handler) CreateClient(
        Action<ResponseMetadata>? onResponse)
    {
        var handler = new MockHttpHandler();
        var authHttpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        var auth = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: "test-secret",
            privateKeyPem: null,
            kid: null,
            tokenUrl: "https://api.test.com/oauth2/token",
            scopes: new[] { "transactions:read" },
            testHttpClient: authHttpClient);

        var apiHttpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        var client = new SignDocsHttpClient(
            apiHttpClient,
            "https://api.test.com",
            TimeSpan.FromSeconds(30),
            auth,
            maxRetries: 0,
            logger: null,
            onResponse: onResponse);

        return (client, handler);
    }
}
