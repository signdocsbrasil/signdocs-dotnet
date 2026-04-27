using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record VerificationResponse(
    [property: JsonPropertyName("evidenceId")] string? EvidenceId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("envelopeId")] string? EnvelopeId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("purpose")] string? Purpose,
    [property: JsonPropertyName("documentHash")] string? DocumentHash,
    [property: JsonPropertyName("evidenceHash")] string? EvidenceHash,
    [property: JsonPropertyName("policy")] Policy? Policy,
    [property: JsonPropertyName("signer")] VerificationSigner? Signer,
    [property: JsonPropertyName("steps")] List<VerificationStep>? Steps,
    [property: JsonPropertyName("tenantName")] string? TenantName,
    [property: JsonPropertyName("tenantCnpj")] string? TenantCnpj,
    [property: JsonPropertyName("createdAt")] string? CreatedAt,
    [property: JsonPropertyName("completedAt")] string? CompletedAt
);

public record VerificationSigner(
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("cpfCnpj")] string? CpfCnpj = null
);

public record VerificationStep(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("order")] int Order,
    [property: JsonPropertyName("completedAt")] string? CompletedAt
);
