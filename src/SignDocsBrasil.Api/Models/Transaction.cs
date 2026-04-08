using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record Transaction(
    [property: JsonPropertyName("tenantId")] string? TenantId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("userExternalId")] string? UserExternalId,
    [property: JsonPropertyName("documentHash")] string? DocumentHash,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("purpose")] string? Purpose,
    [property: JsonPropertyName("policy")] Policy? Policy,
    [property: JsonPropertyName("signer")] Signer? Signer,
    [property: JsonPropertyName("steps")] List<Step>? Steps,
    [property: JsonPropertyName("documentGroupId")] string? DocumentGroupId,
    [property: JsonPropertyName("signerIndex")] int? SignerIndex,
    [property: JsonPropertyName("totalSigners")] int? TotalSigners,
    [property: JsonPropertyName("metadata")] Dictionary<string, string>? Metadata,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt,
    [property: JsonPropertyName("createdAt")] string? CreatedAt,
    [property: JsonPropertyName("updatedAt")] string? UpdatedAt,
    [property: JsonPropertyName("submissionDeadline")] string? SubmissionDeadline,
    [property: JsonPropertyName("deadlineStatus")] string? DeadlineStatus
);
