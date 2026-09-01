using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Request to create a new express signing session.
/// </summary>
public class CreateSigningSessionRequest
{
    /// <summary>
    /// Purpose of the session: <c>DOCUMENT_SIGNATURE</c> or <c>ACTION_AUTHENTICATION</c>.
    /// </summary>
    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; }

    /// <summary>
    /// Identity verification policy for the session.
    /// </summary>
    [JsonPropertyName("policy")]
    public Policy? Policy { get; set; }

    /// <summary>
    /// Signer data. At least one of <c>Cpf</c> or <c>Cnpj</c> is required.
    /// </summary>
    [JsonPropertyName("signer")]
    public Signer? Signer { get; set; }

    /// <summary>
    /// Inline document (base64). Max 10 MB. Supports PDF and other formats.
    /// </summary>
    [JsonPropertyName("document")]
    public SessionDocument? Document { get; set; }

    /// <summary>
    /// Action metadata for <c>ACTION_AUTHENTICATION</c> sessions.
    /// </summary>
    [JsonPropertyName("action")]
    public SessionAction? Action { get; set; }

    /// <summary>
    /// URL invoked after completion. The <c>session_id</c> is appended as a query parameter.
    /// </summary>
    [JsonPropertyName("returnUrl")]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL invoked when the signer cancels the session.
    /// </summary>
    [JsonPropertyName("cancelUrl")]
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Free-form metadata (keys up to 256 chars, values up to 1024 chars).
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Locale for the hosted signing page. One of <c>pt-BR</c>, <c>en</c>, <c>es</c>.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Expiration in minutes (5 to 1440). Default 60.
    /// </summary>
    [JsonPropertyName("expiresInMinutes")]
    public int? ExpiresInMinutes { get; set; }

    /// <summary>
    /// Visual appearance of the hosted signing page.
    /// </summary>
    [JsonPropertyName("appearance")]
    public SessionAppearance? Appearance { get; set; }

    /// <summary>
    /// Identity of the requester. See <see cref="Models.Owner"/> for behavior when set.
    /// </summary>
    [JsonPropertyName("owner")]
    public Owner? Owner { get; set; }

    /// <summary>
    /// Biometric reference image for this session. See <see cref="SessionReferenceImage"/>.
    /// </summary>
    [JsonPropertyName("referenceImage")]
    public SessionReferenceImage? ReferenceImage { get; set; }

    /// <summary>
    /// Biometric reference image, base64 JPEG, max 5MB.
    /// </summary>
    /// <remarks>
    /// Decides which face the BIOMETRIC_MATCH step compares the captured
    /// liveness against. When set, it takes precedence over the user's stored
    /// enrolment — which is what lets a session be signed by someone who was
    /// never enrolled.
    /// </remarks>
    public class SessionReferenceImage
    {
        /// <summary>Reference image, base64-encoded JPEG.</summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    /// <summary>
    /// Inline document for a signing session.
    /// </summary>
    public class SessionDocument
    {
        /// <summary>Document content, base64-encoded.</summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>Original filename.</summary>
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }
    }

    /// <summary>
    /// Action metadata for ACTION_AUTHENTICATION sessions.
    /// </summary>
    public class SessionAction
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }

    /// <summary>
    /// Visual appearance of the hosted signing page.
    /// </summary>
    public class SessionAppearance
    {
        [JsonPropertyName("brandColor")]
        public string? BrandColor { get; set; }

        [JsonPropertyName("logoUrl")]
        public string? LogoUrl { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("backgroundColor")]
        public string? BackgroundColor { get; set; }

        [JsonPropertyName("textColor")]
        public string? TextColor { get; set; }

        [JsonPropertyName("buttonTextColor")]
        public string? ButtonTextColor { get; set; }

        [JsonPropertyName("borderRadius")]
        public string? BorderRadius { get; set; }

        [JsonPropertyName("headerStyle")]
        public string? HeaderStyle { get; set; }

        [JsonPropertyName("fontFamily")]
        public string? FontFamily { get; set; }
    }
}
