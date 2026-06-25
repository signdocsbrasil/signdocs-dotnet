using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class VerificationResource
{
    private readonly SignDocsHttpClient _client;

    internal VerificationResource(SignDocsHttpClient client) => _client = client;

    public async Task<VerificationResponse?> VerifyAsync(
        string evidenceId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestNoAuthAsync<VerificationResponse>(
            HttpMethod.Get,
            $"/v1/verify/{evidenceId}",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<VerificationDownloadsResponse?> DownloadsAsync(
        string evidenceId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestNoAuthAsync<VerificationDownloadsResponse>(
            HttpMethod.Get,
            $"/v1/verify/{evidenceId}/downloads",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Verifies a multi-signer envelope by its ID. Returns envelope status,
    /// the list of signers (each with an <c>EvidenceId</c> for drill-down via
    /// <see cref="VerifyAsync"/>), and consolidated download URLs. For
    /// non-PDF envelopes signed with digital certificates, the consolidated
    /// <c>.p7s</c> containing every signer's <c>SignerInfo</c> is exposed via
    /// <see cref="EnvelopeVerificationDownloads.ConsolidatedSignature"/>.
    /// This endpoint does not require authentication.
    /// </summary>
    public async Task<EnvelopeVerificationResponse?> VerifyEnvelopeAsync(
        string envelopeId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestNoAuthAsync<EnvelopeVerificationResponse>(
            HttpMethod.Get,
            $"/v1/verify/envelope/{envelopeId}",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Inspects an arbitrary PDF for embedded signatures (PAdES, PKCS#7, legacy,
    /// or ICP-Brasil digital certificate) and reports what was detected. Unlike
    /// the other verification methods, this endpoint is <strong>authenticated</strong>:
    /// a Bearer token is sent and the <c>verification:write</c> scope is required.
    /// It is available with <strong>production credentials only</strong>.
    /// </summary>
    /// <param name="request">
    /// The document to inspect; <see cref="VerifyDocumentRequest.Content"/> must be
    /// the base64-encoded PDF.
    /// </param>
    /// <param name="timeout">Optional per-request timeout override.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<VerifyDocumentResponse?> VerifyDocumentAsync(
        VerifyDocumentRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<VerifyDocumentResponse>(
            HttpMethod.Post,
            "/v1/verify/document",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
