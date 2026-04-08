using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class PresignRequest
{
    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}
