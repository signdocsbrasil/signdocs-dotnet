using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class DocumentGroupsResource
{
    private readonly SignDocsHttpClient _client;

    internal DocumentGroupsResource(SignDocsHttpClient client) => _client = client;

    public async Task<CombinedStampResponse?> CombinedStampAsync(
        string groupId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<CombinedStampResponse>(
            HttpMethod.Post,
            $"/v1/document-groups/{groupId}/combined-stamp",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
