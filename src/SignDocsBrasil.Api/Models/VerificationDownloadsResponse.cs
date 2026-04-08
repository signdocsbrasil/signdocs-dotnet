using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record VerificationDownloadsResponse(
    [property: JsonPropertyName("evidenceId")] string? EvidenceId,
    [property: JsonPropertyName("downloads")] Downloads? Downloads
);

public record Downloads(
    [property: JsonPropertyName("evidencePack")] DownloadArtifact? EvidencePack,
    [property: JsonPropertyName("signedPdf")] DownloadArtifact? SignedPdf,
    [property: JsonPropertyName("finalPdf")] DownloadArtifact? FinalPdf
);

public record DownloadArtifact(
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("filename")] string? Filename
);
