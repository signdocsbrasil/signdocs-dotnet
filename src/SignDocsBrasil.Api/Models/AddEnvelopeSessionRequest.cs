using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class AddEnvelopeSessionRequest
{
    [JsonPropertyName("signerName")]
    public string? SignerName { get; set; }

    [JsonPropertyName("signerUserExternalId")]
    public string SignerUserExternalId { get; set; } = "sdk";

    [JsonPropertyName("signerCpf")]
    public string? SignerCpf { get; set; }

    [JsonPropertyName("signerCnpj")]
    public string? SignerCnpj { get; set; }

    [JsonPropertyName("signerEmail")]
    public string? SignerEmail { get; set; }

    [JsonPropertyName("signerPhone")]
    public string? SignerPhone { get; set; }

    [JsonPropertyName("policyProfile")]
    public string PolicyProfile { get; set; } = "CLICK_ONLY";

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = "DOCUMENT_SIGNATURE";

    [JsonPropertyName("signerIndex")]
    public int SignerIndex { get; set; } = 1;

    [JsonPropertyName("returnUrl")]
    public string? ReturnUrl { get; set; }

    [JsonPropertyName("cancelUrl")]
    public string? CancelUrl { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}
