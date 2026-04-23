using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Request to add a signer session to an envelope.
/// </summary>
public class AddEnvelopeSessionRequest
{
    /// <summary>
    /// Signer data. At least one of <c>Cpf</c> or <c>Cnpj</c> is required.
    /// </summary>
    [JsonPropertyName("signer")]
    public EnvelopeSessionSigner? Signer { get; set; }

    /// <summary>
    /// Identity verification policy for the session.
    /// </summary>
    [JsonPropertyName("policy")]
    public EnvelopeSessionPolicy? Policy { get; set; }

    /// <summary>
    /// Session purpose: <c>DOCUMENT_SIGNATURE</c> (default) or <c>ACTION_AUTHENTICATION</c>.
    /// </summary>
    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; }

    /// <summary>
    /// Signer index within the envelope (minimum 1). Determines order in SEQUENTIAL mode.
    /// </summary>
    [JsonPropertyName("signerIndex")]
    public int SignerIndex { get; set; }

    /// <summary>
    /// Overrides the envelope's <c>returnUrl</c> for this session.
    /// </summary>
    [JsonPropertyName("returnUrl")]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Overrides the envelope's <c>cancelUrl</c> for this session.
    /// </summary>
    [JsonPropertyName("cancelUrl")]
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Free-form metadata specific to this session.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Signer data for an envelope session.
    /// </summary>
    public class EnvelopeSessionSigner
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("userExternalId")]
        public string? UserExternalId { get; set; }

        [JsonPropertyName("cpf")]
        public string? Cpf { get; set; }

        [JsonPropertyName("cnpj")]
        public string? Cnpj { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("birthDate")]
        public string? BirthDate { get; set; }

        [JsonPropertyName("otpChannel")]
        public string? OtpChannel { get; set; }
    }

    /// <summary>
    /// Identity verification policy for an envelope session.
    /// </summary>
    public class EnvelopeSessionPolicy
    {
        [JsonPropertyName("profile")]
        public string? Profile { get; set; }
    }
}
