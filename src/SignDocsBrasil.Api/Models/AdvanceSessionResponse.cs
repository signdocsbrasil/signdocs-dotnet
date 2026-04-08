using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record AdvanceSessionResponse(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("currentStep")] AdvanceSessionResponse.StepRef? CurrentStep = null,
    [property: JsonPropertyName("nextStep")] AdvanceSessionResponse.StepRef? NextStep = null,
    [property: JsonPropertyName("evidenceId")] string? EvidenceId = null,
    [property: JsonPropertyName("redirectUrl")] string? RedirectUrl = null,
    [property: JsonPropertyName("completedAt")] string? CompletedAt = null,
    [property: JsonPropertyName("hostedUrl")] string? HostedUrl = null,
    [property: JsonPropertyName("livenessSessionId")] string? LivenessSessionId = null,
    [property: JsonPropertyName("signatureRequestId")] string? SignatureRequestId = null,
    [property: JsonPropertyName("hashToSign")] string? HashToSign = null,
    [property: JsonPropertyName("hashAlgorithm")] string? HashAlgorithm = null,
    [property: JsonPropertyName("signatureAlgorithm")] string? SignatureAlgorithm = null,
    [property: JsonPropertyName("sandbox")] AdvanceSessionResponse.SandboxInfo? Sandbox = null
)
{
    public record StepRef(
        [property: JsonPropertyName("stepId")] string? StepId,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("status")] string? Status = null
    );

    public record SandboxInfo(
        [property: JsonPropertyName("otpCode")] string? OtpCode = null
    );
}
