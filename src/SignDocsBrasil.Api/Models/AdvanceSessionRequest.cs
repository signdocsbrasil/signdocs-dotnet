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
    [property: JsonPropertyName("geolocation")] AdvanceSessionRequest.GeolocationData? Geolocation = null
)
{
    public record GeolocationData(
        [property: JsonPropertyName("latitude")] double? Latitude = null,
        [property: JsonPropertyName("longitude")] double? Longitude = null,
        [property: JsonPropertyName("accuracy")] double? Accuracy = null,
        [property: JsonPropertyName("source")] string? Source = null
    );
}
