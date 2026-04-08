using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record EnvelopeSessionSummary(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("signerIndex")] int SignerIndex,
    [property: JsonPropertyName("signerName")] string? SignerName,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("completedAt")] string? CompletedAt,
    [property: JsonPropertyName("evidenceId")] string? EvidenceId
);
