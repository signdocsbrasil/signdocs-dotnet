using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record StepResult(
    [property: JsonPropertyName("liveness")] LivenessResult? Liveness,
    [property: JsonPropertyName("match")] MatchResult? Match,
    [property: JsonPropertyName("otp")] OtpResult? Otp,
    [property: JsonPropertyName("click")] ClickResult? Click,
    [property: JsonPropertyName("purposeDisclosure")] PurposeDisclosureResult? PurposeDisclosure,
    [property: JsonPropertyName("digitalSignature")] DigitalSignatureResult? DigitalSignature,
    [property: JsonPropertyName("serproIdentity")] SerproIdentityResult? SerproIdentity,
    [property: JsonPropertyName("geolocation")] GeolocationResult? Geolocation,
    [property: JsonPropertyName("documentPhotoMatch")] DocumentPhotoMatchResult? DocumentPhotoMatch,
    [property: JsonPropertyName("quality")] QualityResult? Quality,
    [property: JsonPropertyName("governmentDbValidation")] GovernmentDbValidation? GovernmentDbValidation,
    [property: JsonPropertyName("providerTimestamp")] string? ProviderTimestamp
);

public record LivenessResult(
    [property: JsonPropertyName("confidence")] double Confidence,
    [property: JsonPropertyName("provider")] string? Provider,
    [property: JsonPropertyName("captureMode")] string? CaptureMode,
    [property: JsonPropertyName("complianceStandards")] List<string>? ComplianceStandards
);

public record MatchResult(
    [property: JsonPropertyName("similarity")] double Similarity,
    [property: JsonPropertyName("threshold")] double Threshold
);

public record OtpResult(
    [property: JsonPropertyName("verified")] bool Verified,
    [property: JsonPropertyName("channel")] string? Channel
);

public record ClickResult(
    [property: JsonPropertyName("accepted")] bool Accepted,
    [property: JsonPropertyName("textVersion")] string? TextVersion
);

public record DigitalSignatureResult(
    [property: JsonPropertyName("certificateSubject")] string? CertificateSubject,
    [property: JsonPropertyName("certificateSerial")] string? CertificateSerial,
    [property: JsonPropertyName("certificateIssuer")] string? CertificateIssuer,
    [property: JsonPropertyName("algorithm")] string? Algorithm,
    [property: JsonPropertyName("signedAt")] string? SignedAt,
    [property: JsonPropertyName("signedPdfHash")] string? SignedPdfHash,
    [property: JsonPropertyName("signedPdfS3Key")] string? SignedPdfS3Key,
    [property: JsonPropertyName("signatureFieldName")] string? SignatureFieldName
);

public record PurposeDisclosureResult(
    [property: JsonPropertyName("acknowledged")] bool Acknowledged,
    [property: JsonPropertyName("disclosureTextHash")] string? DisclosureTextHash,
    [property: JsonPropertyName("disclosureVersion")] string? DisclosureVersion,
    [property: JsonPropertyName("notificationChannel")] string? NotificationChannel,
    [property: JsonPropertyName("notificationSentAt")] string? NotificationSentAt
);

public record SerproIdentityResult(
    [property: JsonPropertyName("valid")] bool Valid,
    [property: JsonPropertyName("provider")] string? Provider,
    [property: JsonPropertyName("nameMatch")] bool NameMatch,
    [property: JsonPropertyName("birthDateMatch")] bool BirthDateMatch,
    [property: JsonPropertyName("biometricMatch")] bool BiometricMatch,
    [property: JsonPropertyName("biometricConfidence")] double BiometricConfidence,
    [property: JsonPropertyName("governmentDatabase")] GovernmentDatabase? GovernmentDatabase = null
);

public record GeolocationResult(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("accuracy")] double? Accuracy,
    [property: JsonPropertyName("source")] string? Source
);

public record DocumentPhotoMatchResult(
    [property: JsonPropertyName("documentType")] string? DocumentType,
    [property: JsonPropertyName("extractedFaceHash")] string? ExtractedFaceHash,
    [property: JsonPropertyName("similarity")] double Similarity,
    [property: JsonPropertyName("threshold")] double Threshold,
    [property: JsonPropertyName("faceExtractionConfidence")] double FaceExtractionConfidence,
    [property: JsonPropertyName("biographicValidation")] BiographicValidation? BiographicValidation
);

public record BiographicValidation(
    [property: JsonPropertyName("nameMatch")] bool? NameMatch,
    [property: JsonPropertyName("cpfMatch")] bool? CpfMatch,
    [property: JsonPropertyName("birthDateMatch")] bool? BirthDateMatch,
    [property: JsonPropertyName("overallValid")] bool OverallValid,
    [property: JsonPropertyName("matchedFields")] List<string>? MatchedFields,
    [property: JsonPropertyName("unmatchedFields")] List<string>? UnmatchedFields
);

public record QualityResult(
    [property: JsonPropertyName("brightness")] double Brightness,
    [property: JsonPropertyName("sharpness")] double Sharpness,
    [property: JsonPropertyName("faceAreaRatio")] double FaceAreaRatio
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GovernmentDatabase
{
    SERPRO_DATAVALID,
    TSE,
    IDRC
}

public record GovernmentDbValidation(
    [property: JsonPropertyName("database")] GovernmentDatabase Database,
    [property: JsonPropertyName("validatedAt")] string ValidatedAt,
    [property: JsonPropertyName("cpfHash")] string CpfHash,
    [property: JsonPropertyName("biometricScore")] double BiometricScore,
    [property: JsonPropertyName("cached")] bool Cached,
    [property: JsonPropertyName("cacheVerifySimilarity")] double? CacheVerifySimilarity = null,
    [property: JsonPropertyName("cacheExpiresAt")] string? CacheExpiresAt = null
);
