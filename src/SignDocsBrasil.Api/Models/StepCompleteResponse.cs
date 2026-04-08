using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record StepCompleteResponse(
    [property: JsonPropertyName("stepId")] string? StepId,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("attempts")] int Attempts,
    [property: JsonPropertyName("result")] StepResult? Result
);
