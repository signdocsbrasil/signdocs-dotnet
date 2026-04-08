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
    public async Task<EnvelopeSession?> AddSessionAsync(
        string envelopeId,
        AddEnvelopeSessionRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<EnvelopeSession>(
            HttpMethod.Post,
            $"/v1/envelopes/{envelopeId}/sessions",
            body: request,
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
