using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record DownloadResponse(
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("documentHash")] string? DocumentHash,
    [property: JsonPropertyName("originalUrl")] string? OriginalUrl,
    [property: JsonPropertyName("signedUrl")] string? SignedUrl,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn
);
