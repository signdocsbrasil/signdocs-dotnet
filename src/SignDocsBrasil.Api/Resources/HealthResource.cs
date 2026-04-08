using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class HealthResource
{
    private readonly SignDocsHttpClient _client;

    internal HealthResource(SignDocsHttpClient client) => _client = client;

    public async Task<HealthCheckResponse?> CheckAsync(
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestNoAuthAsync<HealthCheckResponse>(
            HttpMethod.Get,
            "/health",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<HealthHistoryResponse?> HistoryAsync(
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestNoAuthAsync<HealthHistoryResponse>(
            HttpMethod.Get,
            "/health/history",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
