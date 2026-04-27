using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record WebhookTestDelivery(
    [property: JsonPropertyName("httpStatus")] int HttpStatus,
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("error")] string? Error = null
);

public record WebhookTestResponse(
    [property: JsonPropertyName("webhookId")] string WebhookId,
    [property: JsonPropertyName("testDelivery")] WebhookTestDelivery TestDelivery
);
