using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class CreateEnvelopeRequest
{
    [JsonPropertyName("signingMode")]
    public string? SigningMode { get; set; }

    [JsonPropertyName("totalSigners")]
    public int TotalSigners { get; set; }

    [JsonPropertyName("documentContent")]
    public string? DocumentContent { get; set; }

    [JsonPropertyName("documentFilename")]
    public string? DocumentFilename { get; set; }

    [JsonPropertyName("returnUrl")]
    public string? ReturnUrl { get; set; }

    [JsonPropertyName("cancelUrl")]
    public string? CancelUrl { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("expiresInMinutes")]
    public int? ExpiresInMinutes { get; set; }
}
