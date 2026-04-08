using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record CombinedStampResponse(
    [property: JsonPropertyName("groupId")] string? GroupId,
    [property: JsonPropertyName("signerCount")] int SignerCount,
    [property: JsonPropertyName("downloadUrl")] string? DownloadUrl,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn
);
