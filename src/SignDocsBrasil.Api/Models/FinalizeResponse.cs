using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record FinalizeResponse(
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("evidenceId")] string? EvidenceId,
    [property: JsonPropertyName("evidenceHash")] string? EvidenceHash,
    [property: JsonPropertyName("completedAt")] string? CompletedAt
);
