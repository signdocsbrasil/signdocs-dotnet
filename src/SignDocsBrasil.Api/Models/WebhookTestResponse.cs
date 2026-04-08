using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record WebhookTestResponse(
    [property: JsonPropertyName("deliveryId")] string? DeliveryId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("statusCode")] int? StatusCode
);
