using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>One row of a batch enrollment.</summary>
public record BatchEnrollmentItem(
    [property: JsonPropertyName("userExternalId")] string? UserExternalId,
    [property: JsonPropertyName("image")] string? Image,
    [property: JsonPropertyName("cpf")] string? Cpf,
    [property: JsonPropertyName("source")] string? Source = null
);

/// <summary>Request body for POST /v1/users/enrollments.</summary>
public record EnrollUsersBatchRequest(
    [property: JsonPropertyName("enrollments")] List<BatchEnrollmentItem> Enrollments,

    // Inspect without writing. Every row is evaluated and returned with quality
    // metrics, and nothing is persisted — no image reaches storage, no record is
    // created, and the 90-day retention clock never starts.
    //
    // Rekognition's confidence answers "is this a face?", not "is this a good
    // reference": a dark, blurred photo enrols happily at 99.99 confidence and
    // then fails face matching months later, one employee at a time. A dry run
    // surfaces that while the batch is still in front of you. It costs the same
    // one Rekognition call per row that enrolling would.
    [property: JsonPropertyName("dryRun")] bool? DryRun = null
);

/// <summary>Rekognition's 0-100 measures for the detected face. Dry run only.</summary>
public record FaceQualityMetrics(
    [property: JsonPropertyName("brightness")] double? Brightness = null,
    [property: JsonPropertyName("sharpness")] double? Sharpness = null
);

/// <summary>Head rotation in degrees. Dry run only.</summary>
public record FacePoseMetrics(
    [property: JsonPropertyName("yaw")] double? Yaw = null,
    [property: JsonPropertyName("pitch")] double? Pitch = null,
    [property: JsonPropertyName("roll")] double? Roll = null
);

/// <summary>One row's outcome.</summary>
public record BatchEnrollmentResult(
    [property: JsonPropertyName("index")] int Index,

    // "enrolled"/"failed" on a real write; "usable"/"marginal"/"rejected" on a
    // dry run. "marginal" is the one to act on: it would enrol without complaint
    // today and is exactly what becomes a rejected signature later.
    [property: JsonPropertyName("status")] string? Status,

    [property: JsonPropertyName("userExternalId")] string? UserExternalId = null,
    [property: JsonPropertyName("error")] string? Error = null,
    [property: JsonPropertyName("enrollmentVersion")] int? EnrollmentVersion = null,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt = null,
    [property: JsonPropertyName("faceConfidence")] double? FaceConfidence = null,
    [property: JsonPropertyName("quality")] FaceQualityMetrics? Quality = null,
    [property: JsonPropertyName("pose")] FacePoseMetrics? Pose = null,
    // Face area as a fraction of the frame, 0-1. Dry run only.
    [property: JsonPropertyName("faceCoverage")] double? FaceCoverage = null,
    // Empty on a clean photo.
    [property: JsonPropertyName("warnings")] List<string>? Warnings = null,

    // Whether the photo works as a reference. Separate from Status, which says
    // what happened to the write: a row can be Status "enrolled" with
    // ReferenceQuality "marginal".
    [property: JsonPropertyName("referenceQuality")] string? ReferenceQuality = null
);

/// <summary>Result of a batch enrollment.</summary>
/// <remarks>
/// Partial success is the point, so this comes back 200 even when rows failed:
/// one unusable photo must not reject the other twenty-four. Read Results, not
/// the HTTP status.
/// </remarks>
public record EnrollUsersBatchResponse(
    [property: JsonPropertyName("submitted")] int Submitted,
    [property: JsonPropertyName("results")] List<BatchEnrollmentResult>? Results = null,
    // Real writes only.
    [property: JsonPropertyName("succeeded")] int? Succeeded = null,
    [property: JsonPropertyName("failed")] int? Failed = null,
    // Dry runs only.
    [property: JsonPropertyName("dryRun")] bool? DryRun = null,
    [property: JsonPropertyName("usable")] int? Usable = null,
    [property: JsonPropertyName("marginal")] int? Marginal = null,
    [property: JsonPropertyName("rejected")] int? Rejected = null
);

/// <summary>
/// Advisory reasons a reference photo is usable but weak.
/// </summary>
/// <remarks>
/// A row carrying any of these enrols without complaint today and is exactly
/// what fails face matching later, which is the whole reason the dry run exists.
/// </remarks>
public static class EnrollmentWarning
{
    public const string LowBrightness = "LOW_BRIGHTNESS";
    public const string LowSharpness = "LOW_SHARPNESS";
    public const string FaceTooSmall = "FACE_TOO_SMALL";
    public const string HeadTurned = "HEAD_TURNED";
}
