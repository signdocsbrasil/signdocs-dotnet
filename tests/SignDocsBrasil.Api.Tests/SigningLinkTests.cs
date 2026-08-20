using SignDocsBrasil.Api.Errors;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;
using SignDocsBrasil.Api.Resources;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

/// <summary>
/// Minting a signing link re-issues a single-use URL for an ACTIVE session
/// without creating another transaction and without consuming quota.
/// </summary>
public class SigningLinkTests : IDisposable
{
    private readonly SignDocsHttpClient _client;
    private readonly MockHttpHandler _handler;
    private readonly SigningSessionsResource _sessions;

    public SigningLinkTests()
    {
        (_client, _handler) = TestClientFactory.Create();
        _sessions = new SigningSessionsResource(_client);
    }

    public void Dispose() => _client.Dispose();

    private const string MintedBody = """
    {
      "sessionId": "ss_1",
      "transactionId": "tx_1",
      "url": "https://sign.signdocs.com.br/s/ss_1?cs=abc",
      "expiresAt": "2026-08-27T12:00:00.000Z",
      "expiresIn": 3600
    }
    """;

    [Fact]
    public async Task Link_PostsToTheLinkPath()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, MintedBody);

        MintSigningLinkResponse? result = await _sessions.LinkAsync("ss_1");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal(HttpMethod.Post, apiRequest.Method);
        Assert.Equal("/v1/signing-sessions/ss_1/link", apiRequest.RequestUri!.AbsolutePath);

        Assert.NotNull(result);
        Assert.Equal("https://sign.signdocs.com.br/s/ss_1?cs=abc", result!.Url);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Equal("tx_1", result.TransactionId);
        Assert.Equal("2026-08-27T12:00:00.000Z", result.ExpiresAt);
    }

    [Fact]
    public async Task Link_SendsNoIdempotencyKey()
    {
        // Not a metered create. A key would let a retry replay a URL that has
        // already been consumed instead of issuing the fresh one asked for.
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, MintedBody);

        await _sessions.LinkAsync("ss_1");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.False(apiRequest.Headers.Contains("X-Idempotency-Key"));
    }

    [Fact]
    public async Task Link_ThrowsConflictWhenSessionIsNotActive()
    {
        // A link to a finished session would authenticate nothing.
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(409, """
        {"type":"about:blank","title":"Conflict","status":409,
         "detail":"Session cannot be linked in status: COMPLETED"}
        """);

        await Assert.ThrowsAsync<ConflictException>(() => _sessions.LinkAsync("ss_done"));
    }
}
