using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record Envelope(
    [property: JsonPropertyName("envelopeId")] string? EnvelopeId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("signingMode")] string? SigningMode,
    [property: JsonPropertyName("totalSigners")] int TotalSigners,
    [property: JsonPropertyName("documentHash")] string? DocumentHash,
    [property: JsonPropertyName("createdAt")] string? CreatedAt,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt
);
