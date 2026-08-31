using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record AdvanceSessionRequest(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("otpCode")] string? OtpCode = null,
    [property: JsonPropertyName("otpChannel")] string? OtpChannel = null,
    [property: JsonPropertyName("livenessSessionId")] string? LivenessSessionId = null,
    [property: JsonPropertyName("certificateChainPems")] List<string>? CertificateChainPems = null,
    [property: JsonPropertyName("signatureRequestId")] string? SignatureRequestId = null,
    [property: JsonPropertyName("rawSignatureBase64")] string? RawSignatureBase64 = null,
    [property: JsonPropertyName("geolocation")] AdvanceSessionRequest.GeolocationData? Geolocation = null,

    // CPF or CNPJ the signer types to confirm their identity (confirm_signer).
    [property: JsonPropertyName("cpfCnpj")] string? CpfCnpj = null,

    // Base64 identity-document photo, max 5MB (complete_document_photo).
    [property: JsonPropertyName("documentImage")] string? DocumentImage = null,

    [property: JsonPropertyName("documentType")] string? DocumentType = null,

    // Sandbox-only simulated scores, so a rejection can be rehearsed. Read only
    // once the step already resolved to sandbox — they can never make a real
    // verification pass.
    [property: JsonPropertyName("sandboxSimilarity")] double? SandboxSimilarity = null,
    [property: JsonPropertyName("sandboxLivenessConfidence")] double? SandboxLivenessConfidence = null,
    [property: JsonPropertyName("sandboxBrightness")] double? SandboxBrightness = null,
    [property: JsonPropertyName("sandboxSharpness")] double? SandboxSharpness = null,

    [property: JsonPropertyName("deviceInfo")] AdvanceSessionRequest.DeviceInfoData? DeviceInfo = null
)
{
    // Device characteristics, recorded in the evidence alongside geolocation.
    public record DeviceInfoData(
        [property: JsonPropertyName("screenWidth")] int? ScreenWidth = null,
        [property: JsonPropertyName("screenHeight")] int? ScreenHeight = null,
        [property: JsonPropertyName("language")] string? Language = null,
        [property: JsonPropertyName("platform")] string? Platform = null,
        [property: JsonPropertyName("touchPoints")] int? TouchPoints = null
    );

    public record GeolocationData(
        [property: JsonPropertyName("latitude")] double? Latitude = null,
        [property: JsonPropertyName("longitude")] double? Longitude = null,
        [property: JsonPropertyName("accuracy")] double? Accuracy = null,
        [property: JsonPropertyName("source")] string? Source = null
    );
}
