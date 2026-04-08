using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class UsersResource
{
    private readonly SignDocsHttpClient _client;

    internal UsersResource(SignDocsHttpClient client) => _client = client;

    public async Task<EnrollUserResponse?> EnrollAsync(
        string userExternalId,
        EnrollUserRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<EnrollUserResponse>(
            HttpMethod.Put,
            $"/v1/users/{userExternalId}/enrollment",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
