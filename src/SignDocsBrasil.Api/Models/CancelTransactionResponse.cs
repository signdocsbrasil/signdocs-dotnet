using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record CancelTransactionResponse(
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("cancelledAt")] string? CancelledAt
);
