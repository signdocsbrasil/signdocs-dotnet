using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class SigningResource
{
    private readonly SignDocsHttpClient _client;

    internal SigningResource(SignDocsHttpClient client) => _client = client;

    public async Task<PrepareSigningResponse?> PrepareAsync(
        string transactionId,
        PrepareSigningRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<PrepareSigningResponse>(
            HttpMethod.Post,
            $"/v1/transactions/{transactionId}/signing/prepare",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<CompleteSigningResponse?> CompleteAsync(
        string transactionId,
        CompleteSigningRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<CompleteSigningResponse>(
            HttpMethod.Post,
            $"/v1/transactions/{transactionId}/signing/complete",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
