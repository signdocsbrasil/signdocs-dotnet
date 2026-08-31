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
    [property: JsonPropertyName("sandbox")] AdvanceSessionResponse.SandboxInfo? Sandbox = null,

    // Why a step was rejected, when the step fails but the request does not.
    // This is the part that matters most in a biometric integration: a rejected
    // step comes back 200 with the session still ACTIVE and the reason
    // here, not as an HTTP error. Code that only catches exceptions from the
    // call reads a rejection as success.
    // Emitted today: BIOMETRIC_MATCH_FAILED, LIVENESS_NOT_COMPLETED,
    // DOCUMENT_QUALITY_LOW, DOCUMENT_MATCH_FAILED and the SERPRO_* family.
    [property: JsonPropertyName("errorCode")] string? ErrorCode = null,

    // pt-BR text addressed to the signer, ready to display.
    [property: JsonPropertyName("errorDetail")] string? ErrorDetail = null,

    // True while the step has attempts left. Once they run out the step goes
    // FAILED and this is false — the signal that retrying will not help. Each
    // retry is billed as overage.
    [property: JsonPropertyName("retryable")] bool? Retryable = null,

    // Set when the policy diverted to an alternative step.
    [property: JsonPropertyName("fallback")] AdvanceSessionResponse.FallbackInfo? Fallback = null
)
{
    public record FallbackInfo(
        [property: JsonPropertyName("triggered")] bool Triggered = false,
        [property: JsonPropertyName("reason")] string? Reason = null,
        [property: JsonPropertyName("nextStepType")] string? NextStepType = null
    );

    public record StepRef(
        [property: JsonPropertyName("stepId")] string? StepId,
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("status")] string? Status = null
    );

    public record SandboxInfo(
        [property: JsonPropertyName("otpCode")] string? OtpCode = null,
        // The biometric step will be approved automatically.
        [property: JsonPropertyName("autoPass")] bool? AutoPass = null
    );
}
