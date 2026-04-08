using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record EnvelopeDetail(
    [property: JsonPropertyName("envelopeId")] string? EnvelopeId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("signingMode")] string? SigningMode,
    [property: JsonPropertyName("totalSigners")] int TotalSigners,
    [property: JsonPropertyName("addedSessions")] int AddedSessions,
    [property: JsonPropertyName("completedSessions")] int CompletedSessions,
    [property: JsonPropertyName("documentHash")] string? DocumentHash,
    [property: JsonPropertyName("sessions")] List<EnvelopeSessionSummary>? Sessions,
    [property: JsonPropertyName("createdAt")] string? CreatedAt,
    [property: JsonPropertyName("updatedAt")] string? UpdatedAt,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt,
    [property: JsonPropertyName("combinedSignedPdfUrl")] string? CombinedSignedPdfUrl
);
