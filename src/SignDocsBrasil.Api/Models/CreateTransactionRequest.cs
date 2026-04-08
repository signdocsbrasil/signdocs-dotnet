using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class CreateTransactionRequest
{
    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; }

    [JsonPropertyName("policy")]
    public Policy? Policy { get; set; }

    [JsonPropertyName("signer")]
    public Signer? Signer { get; set; }

    [JsonPropertyName("document")]
    public InlineDocument? Document { get; set; }

    [JsonPropertyName("action")]
    public ActionMetadata? Action { get; set; }

    [JsonPropertyName("digitalSignature")]
    public DigitalSignatureMetadata? DigitalSignature { get; set; }

    [JsonPropertyName("documentGroupId")]
    public string? DocumentGroupId { get; set; }

    [JsonPropertyName("signerIndex")]
    public int? SignerIndex { get; set; }

    [JsonPropertyName("totalSigners")]
    public int? TotalSigners { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("expiresInMinutes")]
    public int? ExpiresInMinutes { get; set; }

    public class InlineDocument
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("filename")]
        public string? Filename { get; set; }
    }

    public class ActionMetadata
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }

    public class DigitalSignatureMetadata
    {
        [JsonPropertyName("signatureFieldName")]
        public string? SignatureFieldName { get; set; }

        [JsonPropertyName("signatureReason")]
        public string? SignatureReason { get; set; }

        [JsonPropertyName("signatureLocation")]
        public string? SignatureLocation { get; set; }
    }
}
