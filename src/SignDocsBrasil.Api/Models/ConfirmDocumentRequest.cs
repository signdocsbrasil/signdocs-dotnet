using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class ConfirmDocumentRequest
{
    [JsonPropertyName("uploadToken")]
    public string? UploadToken { get; set; }
}
