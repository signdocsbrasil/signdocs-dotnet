using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Lightweight signing session status used for polling.
/// </summary>
public record SigningSessionStatus(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("completedAt")] string? CompletedAt = null,
    [property: JsonPropertyName("evidenceId")] string? EvidenceId = null
);
