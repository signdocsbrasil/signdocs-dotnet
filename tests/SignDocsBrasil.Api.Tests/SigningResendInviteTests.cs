using SignDocsBrasil.Api.Errors;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;
using SignDocsBrasil.Api.Resources;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

/// <summary>
/// Resending an invite sends a new link to the signer of an ACTIVE session, on the
/// channels the signer was created with or the ones chosen for this call.
/// </summary>
public class SigningResendInviteTests : IDisposable
{
    private readonly SignDocsHttpClient _client;
    private readonly MockHttpHandler _handler;
    private readonly SigningSessionsResource _sessions;

    public SigningResendInviteTests()
    {
        (_client, _handler) = TestClientFactory.Create();
        _sessions = new SigningSessionsResource(_client);
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task ResendInvite_SendsNoChannelsAndAGeneratedKeyByDefault()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """
        {"sessionId":"ss_1","transactionId":"tx_1","to":"signer@example.com","resentAt":"2026-11-04T12:00:00.000Z"}
        """);

        ResendSigningInviteResponse? result = await _sessions.ResendInviteAsync("ss_1");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal(HttpMethod.Post, apiRequest.Method);
        Assert.Equal("/v1/signing-sessions/ss_1/resend-invite", apiRequest.RequestUri!.AbsolutePath);
        // The client sends {} on a POST without a body; the API reads it as no channels chosen.
        Assert.Equal("{}", _handler.RequestBodies[1]);
        // With no key passed the client mints one, so its automatic retries share it.
        Assert.True(apiRequest.Headers.Contains("X-Idempotency-Key"));

        Assert.NotNull(result);
        Assert.Equal("tx_1", result!.TransactionId);
        Assert.Equal("signer@example.com", result.To);
        Assert.Equal("2026-11-04T12:00:00.000Z", result.ResentAt);
        Assert.Null(result.WhatsAppInviteSent);
        Assert.Null(result.TelegramInviteSent);
    }

    [Fact]
    public async Task ResendInvite_SendsTheGivenKey()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """
        {"sessionId":"ss_1","transactionId":"tx_1","resentAt":"2026-11-04T12:00:00.000Z"}
        """);

        await _sessions.ResendInviteAsync("ss_1", idempotencyKey: "idem-1");

        Assert.Equal("idem-1", _handler.Requests[1].Headers.GetValues("X-Idempotency-Key").Single());
    }

    [Fact]
    public async Task ResendInvite_SendsChannelsAndKeepsTelegramFalse()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """
        {"sessionId":"ss_1","transactionId":"tx_1","whatsappInviteSent":true,"telegramInviteSent":false,"resentAt":"2026-11-04T12:00:00.000Z"}
        """);

        ResendSigningInviteResponse? result = await _sessions.ResendInviteAsync(
            "ss_1", new ResendSigningInviteRequest(new List<string> { "whatsapp", "telegram" }));

        Assert.Equal("""{"deliverVia":["whatsapp","telegram"]}""", _handler.RequestBodies[1]);
        Assert.Null(result!.To);
        Assert.True(result.WhatsAppInviteSent);
        Assert.False(result.TelegramInviteSent);
    }

    [Fact]
    public async Task ResendInvite_SendsAnEmptyChannelListForTheApiToReject()
    {
        // Dropping it would silently resend on the default channels instead of the 400.
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """
        {"sessionId":"ss_1","transactionId":"tx_1","resentAt":"2026-11-04T12:00:00.000Z"}
        """);

        await _sessions.ResendInviteAsync("ss_1", new ResendSigningInviteRequest(new List<string>()));

        Assert.Equal("""{"deliverVia":[]}""", _handler.RequestBodies[1]);
    }

    [Fact]
    public async Task ResendInvite_ThrowsConflictInsideTheThrottle()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(409, """
        {"type":"about:blank","title":"Conflict","status":409,
         "detail":"Aguarde 42s antes de reenviar"}
        """);

        await Assert.ThrowsAsync<ConflictException>(() => _sessions.ResendInviteAsync("ss_1"));
    }
}
