using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Request body for <see cref="SignDocsBrasil.Api.Resources.VerificationResource.VerifyDocumentAsync"/>.
/// </summary>
public class VerifyDocumentRequest
{
    /// <summary>Base64-encoded PDF document content to inspect for signatures. Required.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>Optional original filename of the document.</summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; set; }
}
