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
);
