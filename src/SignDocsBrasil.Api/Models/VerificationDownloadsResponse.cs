using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record VerificationDownloadsResponse(
    [property: JsonPropertyName("evidenceId")] string? EvidenceId,
    [property: JsonPropertyName("downloads")] Downloads? Downloads
);

/// <summary>
/// Per-evidence download artifacts.
/// </summary>
/// <remarks>
/// <see cref="SignedSignature"/> is the detached PKCS#7 / CMS (.p7s) for
/// digital-cert signing of non-PDF documents. It is only populated by the
/// API for <strong>standalone signing sessions</strong> (single-signer);
/// the field is omitted entirely from the response when the evidence
/// belongs to a multi-signer envelope. Use
/// <see cref="SignDocsBrasil.Api.Resources.VerificationResource.VerifyEnvelopeAsync"/>
/// to retrieve the consolidated envelope-level .p7s instead.
/// </remarks>
public record Downloads(
    [property: JsonPropertyName("originalDocument")] DownloadArtifact? OriginalDocument,
    [property: JsonPropertyName("evidencePack")] DownloadArtifact? EvidencePack,
    [property: JsonPropertyName("finalPdf")] DownloadArtifact? FinalPdf,
    [property: JsonPropertyName("signedSignature")] DownloadArtifact? SignedSignature = null
);

public record DownloadArtifact(
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("filename")] string? Filename
);
