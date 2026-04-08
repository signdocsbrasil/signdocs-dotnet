using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class CompleteSigningRequest
{
    [JsonPropertyName("signatureRequestId")]
    public string? SignatureRequestId { get; set; }

    [JsonPropertyName("rawSignatureBase64")]
    public string? RawSignatureBase64 { get; set; }
}
