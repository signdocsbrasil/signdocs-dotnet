using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record TransactionListResponse(
    [property: JsonPropertyName("transactions")] List<Transaction>? Transactions,
    [property: JsonPropertyName("nextToken")] string? NextToken,
    [property: JsonPropertyName("count")] int Count
);
