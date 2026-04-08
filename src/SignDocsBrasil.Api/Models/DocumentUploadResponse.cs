using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record DocumentUploadResponse(
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("documentHash")] string? DocumentHash,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("uploadedAt")] string? UploadedAt
);
