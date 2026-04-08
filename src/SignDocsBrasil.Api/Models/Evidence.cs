using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record Evidence(
    [property: JsonPropertyName("tenantId")] string? TenantId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("evidenceId")] string? EvidenceId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("signer")] EvidenceSigner? Signer,
    [property: JsonPropertyName("steps")] List<EvidenceStep>? Steps,
    [property: JsonPropertyName("document")] EvidenceDocument? Document,
    [property: JsonPropertyName("createdAt")] string? CreatedAt,
    [property: JsonPropertyName("completedAt")] string? CompletedAt
);

public record EvidenceSigner(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("cpf")] string? Cpf,
    [property: JsonPropertyName("cnpj")] string? Cnpj,
    [property: JsonPropertyName("userExternalId")] string? UserExternalId
);

public record EvidenceStep(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("completedAt")] string? CompletedAt,
    [property: JsonPropertyName("result")] Dictionary<string, object>? Result
);

public record EvidenceDocument(
    [property: JsonPropertyName("hash")] string? Hash,
    [property: JsonPropertyName("filename")] string? Filename
);
