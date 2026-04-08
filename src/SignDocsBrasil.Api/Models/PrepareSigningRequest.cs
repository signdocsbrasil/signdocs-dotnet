using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class PrepareSigningRequest
{
    [JsonPropertyName("certificateChainPems")]
    public List<string>? CertificateChainPems { get; set; }
}
