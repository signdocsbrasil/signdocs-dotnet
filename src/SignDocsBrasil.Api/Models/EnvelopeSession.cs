using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record EnvelopeSession(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("signerIndex")] int SignerIndex,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("clientSecret")] string? ClientSecret,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt
);
