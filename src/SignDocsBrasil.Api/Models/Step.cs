using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record Step(
    [property: JsonPropertyName("tenantId")] string? TenantId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("stepId")] string? StepId,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("order")] int Order,
    [property: JsonPropertyName("attempts")] int Attempts,
    [property: JsonPropertyName("maxAttempts")] int MaxAttempts,
    [property: JsonPropertyName("captureMode")] string? CaptureMode,
    [property: JsonPropertyName("startedAt")] string? StartedAt,
    [property: JsonPropertyName("completedAt")] string? CompletedAt,
    [property: JsonPropertyName("result")] StepResult? Result,
    [property: JsonPropertyName("error")] string? Error
);
