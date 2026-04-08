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
}
