using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record ConfirmDocumentResponse(
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("documentHash")] string? DocumentHash
);
