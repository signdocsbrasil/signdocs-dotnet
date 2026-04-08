using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record StartStepResponse(
    [property: JsonPropertyName("stepId")] string? StepId,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("livenessSessionId")] string? LivenessSessionId,
    [property: JsonPropertyName("hostedUrl")] string? HostedUrl,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("otpCode")] string? OtpCode
);
