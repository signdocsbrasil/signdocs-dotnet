using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Public verification response for a multi-signer envelope, returned by
/// <c>GET /v1/verify/envelope/{envelopeId}</c>.
/// </summary>
/// <remarks>
/// For non-PDF envelopes signed with digital certificates, the consolidated
/// <c>.p7s</c> containing every signer's <c>SignerInfo</c> is exposed via
/// <see cref="EnvelopeVerificationDownloads.ConsolidatedSignature"/>.
/// </remarks>
public record EnvelopeVerificationResponse(
    [property: JsonPropertyName("envelopeId")] string? EnvelopeId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("signingMode")] string? SigningMode,
    [property: JsonPropertyName("totalSigners")] int TotalSigners,
    [property: JsonPropertyName("completedSessions")] int CompletedSessions,
    [property: JsonPropertyName("documentHash")] string? DocumentHash,
    [property: JsonPropertyName("tenantName")] string? TenantName,
    [property: JsonPropertyName("tenantCnpj")] string? TenantCnpj,
    [property: JsonPropertyName("signers")] List<EnvelopeVerificationSigner>? Signers,
    [property: JsonPropertyName("downloads")] EnvelopeVerificationDownloads? Downloads,
    [property: JsonPropertyName("createdAt")] string? CreatedAt,
    [property: JsonPropertyName("completedAt")] string? CompletedAt
);

/// <summary>
/// Per-signer entry within an envelope verification response. The
/// <see cref="EvidenceId"/> is populated for completed signers and can be
/// used with <see cref="SignDocsBrasil.Api.Resources.VerificationResource.VerifyAsync"/>
/// to drill down into the individual evidence record.
/// </summary>
public record EnvelopeVerificationSigner(
    [property: JsonPropertyName("signerIndex")] int SignerIndex,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("cpfCnpj")] string? CpfCnpj,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("policyProfile")] string? PolicyProfile,
    [property: JsonPropertyName("evidenceId")] string? EvidenceId,
    [property: JsonPropertyName("completedAt")] string? CompletedAt
);

/// <summary>
/// Envelope-level consolidated downloads. <see cref="CombinedSignedPdf"/> is
/// populated for PDF envelopes; <see cref="ConsolidatedSignature"/> is the
/// merged <c>.p7s</c> for non-PDF envelopes signed with digital certificates.
/// </summary>
public record EnvelopeVerificationDownloads(
    [property: JsonPropertyName("combinedSignedPdf")] DownloadArtifact? CombinedSignedPdf,
    [property: JsonPropertyName("consolidatedSignature")] DownloadArtifact? ConsolidatedSignature
);
