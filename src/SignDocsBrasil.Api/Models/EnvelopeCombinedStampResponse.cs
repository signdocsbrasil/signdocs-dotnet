using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record EnvelopeCombinedStampResponse(
    [property: JsonPropertyName("envelopeId")] string? EnvelopeId,
    [property: JsonPropertyName("downloadUrl")] string? DownloadUrl,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn,
    [property: JsonPropertyName("signerCount")] int SignerCount
);
