using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record PrepareSigningResponse(
    [property: JsonPropertyName("signatureRequestId")] string? SignatureRequestId,
    [property: JsonPropertyName("hashToSign")] string? HashToSign,
    [property: JsonPropertyName("hashAlgorithm")] string? HashAlgorithm,
    [property: JsonPropertyName("signatureAlgorithm")] string? SignatureAlgorithm
);
