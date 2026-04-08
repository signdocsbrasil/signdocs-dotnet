using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class EvidenceResource
{
    private readonly SignDocsHttpClient _client;

    internal EvidenceResource(SignDocsHttpClient client) => _client = client;

    public async Task<Evidence?> GetAsync(
        string transactionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<Evidence>(
            HttpMethod.Get,
            $"/v1/transactions/{transactionId}/evidence",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
