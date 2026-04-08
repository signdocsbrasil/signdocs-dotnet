using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record CancelSigningSessionResponse(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("cancelledAt")] string? CancelledAt
);
