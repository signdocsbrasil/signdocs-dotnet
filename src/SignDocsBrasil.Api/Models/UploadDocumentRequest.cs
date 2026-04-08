using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class UploadDocumentRequest
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}
