using System.Text.Json.Serialization;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class WebhooksResource
{
    private readonly SignDocsHttpClient _client;

    internal WebhooksResource(SignDocsHttpClient client) => _client = client;

    // GET /v1/webhooks returns { "webhooks": [...], "count": N }.
    // Internal DTO used for deserialization; consumers see a List<Webhook>.
    private sealed class WebhookListEnvelope
    {
        [JsonPropertyName("webhooks")] public List<Webhook>? Webhooks { get; set; }
        [JsonPropertyName("count")] public int Count { get; set; }
    }

    public async Task<RegisterWebhookResponse?> RegisterAsync(
        RegisterWebhookRequest request,
        string? idempotencyKey = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestWithIdempotencyAsync<RegisterWebhookResponse>(
            HttpMethod.Post,
            "/v1/webhooks",
            body: request,
            idempotencyKey: idempotencyKey,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<List<Webhook>?> ListAsync(
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        var envelope = await _client.RequestAsync<WebhookListEnvelope>(
            HttpMethod.Get,
            "/v1/webhooks",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
        return envelope?.Webhooks ?? new List<Webhook>();
    }

    public async Task DeleteAsync(
        string webhookId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        await _client.RequestAsync<object>(
            HttpMethod.Delete,
            $"/v1/webhooks/{webhookId}",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<WebhookTestResponse?> TestAsync(
        string webhookId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<WebhookTestResponse>(
            HttpMethod.Post,
            $"/v1/webhooks/{webhookId}/test",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
