using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record EnrollUserResponse(
    [property: JsonPropertyName("userExternalId")] string? UserExternalId,
    [property: JsonPropertyName("enrollmentHash")] string? EnrollmentHash,
    [property: JsonPropertyName("enrollmentVersion")] int EnrollmentVersion,
    [property: JsonPropertyName("enrollmentSource")] string? EnrollmentSource,
    [property: JsonPropertyName("enrolledAt")] string? EnrolledAt,
    [property: JsonPropertyName("cpf")] string? Cpf,
    [property: JsonPropertyName("faceConfidence")] double? FaceConfidence,
    [property: JsonPropertyName("documentImageHash")] string? DocumentImageHash,
    [property: JsonPropertyName("extractionConfidence")] double? ExtractionConfidence
,
    // Capture metrics for the stored reference. Read Warnings alongside
    // FaceConfidence, which answers "is this a face?" and not "is this a good
    // reference": a dark, blurred photo scores 99.99 there and still fails
    // face matching later.
    [property: JsonPropertyName("quality")] FaceQualityMetrics? Quality = null,
    [property: JsonPropertyName("pose")] FacePoseMetrics? Pose = null,
    [property: JsonPropertyName("faceCoverage")] double? FaceCoverage = null,
    // Present on a successful enrolment too — the photo is stored either way.
    [property: JsonPropertyName("warnings")] List<string>? Warnings = null
);

/// <summary>Verdict for one candidate reference photo, from a dry run.</summary>
/// <remarks>
/// "marginal" is the one to act on: it would enrol without complaint and is
/// exactly what becomes a rejected signature later.
/// </remarks>
public record InspectEnrollmentResponse(
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("warnings")] List<string>? Warnings = null,
    [property: JsonPropertyName("dryRun")] bool? DryRun = null,
    [property: JsonPropertyName("userExternalId")] string? UserExternalId = null,
    [property: JsonPropertyName("error")] string? Error = null,
    [property: JsonPropertyName("faceConfidence")] double? FaceConfidence = null,
    [property: JsonPropertyName("quality")] FaceQualityMetrics? Quality = null,
    [property: JsonPropertyName("pose")] FacePoseMetrics? Pose = null,
    [property: JsonPropertyName("faceCoverage")] double? FaceCoverage = null
);
