using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record CompleteSigningResponse(
    [property: JsonPropertyName("stepId")] string? StepId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("result")] SigningResult? Result
);

public record SigningResult(
    [property: JsonPropertyName("digitalSignature")] DigitalSignatureResult? DigitalSignature
);
