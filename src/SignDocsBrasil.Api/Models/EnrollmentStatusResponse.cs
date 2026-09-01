using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Reports whether a user is enrolled and, crucially, until when.
/// </summary>
/// <remarks>
/// The reference image is hard-deleted by S3 lifecycle RetentionDays after
/// enrolment, while the record outlives it by a grace period. ExpiresAt and
/// Expired are what let an integrator run a re-enrolment sweep instead of
/// discovering the gap as a 422 mid-signature — and the sweep has to happen
/// inside that grace window, because once it passes this route answers 404,
/// which is indistinguishable from "never enrolled".
/// </remarks>
public record EnrollmentStatusResponse(
    [property: JsonPropertyName("userExternalId")] string? UserExternalId,
    [property: JsonPropertyName("enrollmentSource")] string? EnrollmentSource = null,
    [property: JsonPropertyName("enrollmentVersion")] int? EnrollmentVersion = null,
    [property: JsonPropertyName("enrollmentHash")] string? EnrollmentHash = null,
    [property: JsonPropertyName("enrolledAt")] string? EnrolledAt = null,
    // When the reference image is deleted.
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt = null,
    // True once ExpiresAt has passed — re-enrol.
    [property: JsonPropertyName("expired")] bool? Expired = null,
    [property: JsonPropertyName("retentionDays")] int? RetentionDays = null,
    // CPF is masked: this route is enumerable by userExternalId.
    [property: JsonPropertyName("maskedCpf")] string? MaskedCpf = null,
    [property: JsonPropertyName("faceConfidence")] double? FaceConfidence = null,
    [property: JsonPropertyName("documentImageHash")] string? DocumentImageHash = null
);
