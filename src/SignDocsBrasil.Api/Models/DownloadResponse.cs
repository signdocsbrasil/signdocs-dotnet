using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <param name="TransactionId">Transaction identifier.</param>
/// <param name="DocumentHash">Hash of the document.</param>
/// <param name="OriginalUrl">Download URL for the original document.</param>
/// <param name="SignedUrl">
/// Signed/stamped document. Present for PDF transactions
/// (<c>DocumentFormat == "pdf"</c>), where the signature is embedded in the PDF.
/// </param>
/// <param name="ExpiresIn">Expiration time in seconds.</param>
/// <param name="SignatureUrl">
/// Detached CAdES signature (<c>.p7s</c>). Returned instead of
/// <paramref name="SignedUrl"/> for non-PDF transactions
/// (<c>DocumentFormat == "generic"</c>), which cannot carry an embedded signature.
/// <para>
/// Caveat: the API presigns this key without checking that the object exists, so
/// a non-PDF signed under a click/OTP policy still returns a URL here — one that
/// 404s, because only the digital-certificate step writes a <c>.p7s</c>. Branch
/// on the signing policy, not on this field being set.
/// </para>
/// </param>
/// <param name="DocumentFormat">
/// <c>"pdf"</c> or <c>"generic"</c>, derived by the API from the uploaded bytes
/// rather than the filename.
/// </param>
public record DownloadResponse(
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("documentHash")] string? DocumentHash,
    [property: JsonPropertyName("originalUrl")] string? OriginalUrl,
    [property: JsonPropertyName("signedUrl")] string? SignedUrl,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn,
    [property: JsonPropertyName("signatureUrl")] string? SignatureUrl = null,
    [property: JsonPropertyName("documentFormat")] string? DocumentFormat = null
);
