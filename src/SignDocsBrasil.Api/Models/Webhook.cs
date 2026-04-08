using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record Webhook(
    [property: JsonPropertyName("webhookId")] string? WebhookId,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("events")] List<string>? Events,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("createdAt")] string? CreatedAt
);
