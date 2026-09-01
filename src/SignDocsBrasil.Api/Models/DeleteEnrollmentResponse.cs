using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Result of erasing a user's biometric enrolment (LGPD art. 18).
/// </summary>
public record DeleteEnrollmentResponse(
    [property: JsonPropertyName("userExternalId")] string? UserExternalId,
    [property: JsonPropertyName("deleted")] bool? Deleted = null,
    [property: JsonPropertyName("deletedAt")] string? DeletedAt = null,
    [property: JsonPropertyName("enrollmentVersion")] int? EnrollmentVersion = null,
    // Objects removed from storage; every version of each is destroyed.
    [property: JsonPropertyName("objectsDeleted")] int? ObjectsDeleted = null,
    [property: JsonPropertyName("versionsDeleted")] int? VersionsDeleted = null
);
