using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class StepsResource
{
    private readonly SignDocsHttpClient _client;

    internal StepsResource(SignDocsHttpClient client) => _client = client;

    public async Task<StepListResponse?> ListAsync(
        string transactionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<StepListResponse>(
            HttpMethod.Get,
            $"/v1/transactions/{transactionId}/steps",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<StartStepResponse?> StartAsync(
        string transactionId,
        string stepId,
        StartStepRequest? request = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<StartStepResponse>(
            HttpMethod.Post,
            $"/v1/transactions/{transactionId}/steps/{stepId}/start",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<StepCompleteResponse?> CompleteAsync(
        string transactionId,
        string stepId,
        object? body = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<StepCompleteResponse>(
            HttpMethod.Post,
            $"/v1/transactions/{transactionId}/steps/{stepId}/complete",
            body: body,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
