using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record SigningSessionListResponse(
    [property: JsonPropertyName("sessions")] List<SigningSession>? Sessions,
    [property: JsonPropertyName("nextToken")] string? NextToken,
    [property: JsonPropertyName("count")] int Count
);
