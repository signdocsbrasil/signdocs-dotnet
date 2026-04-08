using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record PresignResponse(
    [property: JsonPropertyName("uploadUrl")] string? UploadUrl,
    [property: JsonPropertyName("uploadToken")] string? UploadToken,
    [property: JsonPropertyName("s3Key")] string? S3Key,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn,
    [property: JsonPropertyName("contentType")] string? ContentType,
    [property: JsonPropertyName("instructions")] string? Instructions
);
