using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Result of inspecting an arbitrary PDF for embedded signatures via
/// <see cref="SignDocsBrasil.Api.Resources.VerificationResource.VerifyDocumentAsync"/>.
/// </summary>
public record VerifyDocumentResponse(
    [property: JsonPropertyName("signed")] bool Signed,
    [property: JsonPropertyName("signatureCount")] int SignatureCount,
    [property: JsonPropertyName("signatures")] List<DetectedSignature>? Signatures,
    [property: JsonPropertyName("checkedAt")] string? CheckedAt
);

/// <summary>
/// A single signature detected inside the inspected PDF. <c>Type</c> is one of
/// <c>"pades"</c>, <c>"pkcs7"</c>, <c>"legacy"</c>, or <c>"digital_certificate"</c>.
/// </summary>
public record DetectedSignature(
    [property: JsonPropertyName("method")] string? Method,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("confidence")] double Confidence,
    [property: JsonPropertyName("subFilter")] string? SubFilter = null,
    [property: JsonPropertyName("filter")] string? Filter = null
);
