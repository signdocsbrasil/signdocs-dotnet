using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Request to create a multi-signer envelope.
/// </summary>
public class CreateEnvelopeRequest
{
    /// <summary>
    /// Signing mode: <c>PARALLEL</c> or <c>SEQUENTIAL</c>.
    /// </summary>
    [JsonPropertyName("signingMode")]
    public string? SigningMode { get; set; }

    /// <summary>
    /// Total number of signers expected (minimum 2).
    /// </summary>
    [JsonPropertyName("totalSigners")]
    public int TotalSigners { get; set; }

    /// <summary>
    /// Inline document (base64). Max 10 MB.
    /// </summary>
    [JsonPropertyName("document")]
    public EnvelopeDocument? Document { get; set; }

    /// <summary>
    /// Free-form metadata (keys up to 256 chars, values up to 1024 chars).
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Default locale for sessions in this envelope. One of <c>pt-BR</c>, <c>en</c>, <c>es</c>.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Default return URL inherited by sessions in this envelope.
    /// </summary>
    [JsonPropertyName("returnUrl")]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Default cancel URL inherited by sessions in this envelope.
    /// </summary>
    [JsonPropertyName("cancelUrl")]
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Envelope expiration in minutes (minimum 5). Default 1440 (24h).
    /// </summary>
    [JsonPropertyName("expiresInMinutes")]
    public int? ExpiresInMinutes { get; set; }

    /// <summary>
    /// When set, every <c>AddSession</c> call on this envelope auto-dispatches
    /// an invite email to the signer (when the signer's email differs from
    /// <c>owner.email</c>), and <c>owner.email</c> receives per-signer completion
    /// notifications plus a final "all signed" message. See <see cref="Models.Owner"/>.
    /// </summary>
    [JsonPropertyName("owner")]
    public Owner? Owner { get; set; }

    /// <summary>
    /// Inline document for an envelope.
    /// </summary>
    public class EnvelopeDocument
    {
        /// <summary>Document content, base64-encoded.</summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>Original filename.</summary>
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }
    }
}
