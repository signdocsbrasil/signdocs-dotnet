using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class DocumentsResource
{
    private readonly SignDocsHttpClient _client;

    internal DocumentsResource(SignDocsHttpClient client) => _client = client;

    public async Task<DocumentUploadResponse?> UploadAsync(
        string transactionId,
        UploadDocumentRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<DocumentUploadResponse>(
            HttpMethod.Post,
            $"/v1/transactions/{transactionId}/document",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<PresignResponse?> PresignAsync(
        string transactionId,
        PresignRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<PresignResponse>(
            HttpMethod.Post,
            $"/v1/transactions/{transactionId}/document/presign",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<ConfirmDocumentResponse?> ConfirmAsync(
        string transactionId,
        ConfirmDocumentRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<ConfirmDocumentResponse>(
            HttpMethod.Post,
            $"/v1/transactions/{transactionId}/document/confirm",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<DownloadResponse?> DownloadAsync(
        string transactionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<DownloadResponse>(
            HttpMethod.Get,
            $"/v1/transactions/{transactionId}/document/download",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
