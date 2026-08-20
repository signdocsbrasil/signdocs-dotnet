using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class EnvelopesResource
{
    private readonly SignDocsHttpClient _client;

    internal EnvelopesResource(SignDocsHttpClient client) => _client = client;

    /// <summary>
    /// Creates a new envelope for multi-signer document signing.
    /// An X-Idempotency-Key header is automatically set. Pass an explicit key
    /// to enable safe retries with the same idempotency guarantee.
    /// </summary>
    public async Task<Envelope?> CreateAsync(
        CreateEnvelopeRequest request,
        string? idempotencyKey = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestWithIdempotencyAsync<Envelope>(
            HttpMethod.Post,
            "/v1/envelopes",
            body: request,
            idempotencyKey: idempotencyKey,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the details of an envelope, including session summaries.
    /// </summary>
    public async Task<EnvelopeDetail?> GetAsync(
        string envelopeId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<EnvelopeDetail>(
            HttpMethod.Get,
            $"/v1/envelopes/{envelopeId}",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a signing session to an envelope for a specific signer.
    /// </summary>
    /// <remarks>
    /// An X-Idempotency-Key header is set automatically. It matters more here
    /// than on most calls: this response carries the only copy of
    /// <c>ClientSecret</c>, and the client retries 429/500/503 — so an unkeyed
    /// retry creates a second signer, charges the quota again and sends a
    /// second invitation.
    /// <para>
    /// Pass a distinct key per signer. The API scopes its idempotency cache by
    /// key and resolved path, and every signer on an envelope shares that path,
    /// so one key across the loop returns signer 1's response — and signer 1's
    /// ClientSecret — for signer 2.
    /// </para>
    /// </remarks>
    public async Task<EnvelopeSession?> AddSessionAsync(
        string envelopeId,
        AddEnvelopeSessionRequest request,
        string? idempotencyKey = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestWithIdempotencyAsync<EnvelopeSession>(
            HttpMethod.Post,
            $"/v1/envelopes/{envelopeId}/sessions",
            body: request,
            idempotencyKey: idempotencyKey,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Cancels an entire envelope.
    /// </summary>
    /// <remarks>
    /// Transitions every non-terminal session and its transaction to CANCELLED and
    /// marks the envelope CANCELLED, killing the pending signing links. Signatures
    /// already collected are preserved and reported as PreservedSignedCount.
    /// <para>
    /// Prefer this over cancelling each session individually: it is one call, it
    /// records the cancellation as a single auditable terminal event, and it is the
    /// only way to move the envelope's own status. Cancelling the member sessions
    /// one by one leaves the envelope itself ACTIVE.
    /// </para>
    /// <para>
    /// Idempotent: re-cancelling returns CancelledCount 0 and AlreadyCancelled true.
    /// </para>
    /// </remarks>
    /// <param name="envelopeId">The envelope identifier.</param>
    /// <param name="reason">
    /// Free-text reason recorded in the audit trail. Null lets the API default it
    /// to <c>envelope_cancelled</c>.
    /// </param>
    /// <param name="timeout">Optional per-request timeout.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<CancelEnvelopeResponse?> CancelAsync(
        string envelopeId,
        string? reason = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        var body = string.IsNullOrEmpty(reason)
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { ["reason"] = reason! };

        return await _client.RequestAsync<CancelEnvelopeResponse>(
            HttpMethod.Post,
            $"/v1/envelopes/{envelopeId}/cancel",
            body: body,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates a combined stamp PDF for a completed envelope with all signer evidence.
    /// </summary>
    public async Task<EnvelopeCombinedStampResponse?> CombinedStampAsync(
        string envelopeId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<EnvelopeCombinedStampResponse>(
            HttpMethod.Post,
            $"/v1/envelopes/{envelopeId}/combined-stamp",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
