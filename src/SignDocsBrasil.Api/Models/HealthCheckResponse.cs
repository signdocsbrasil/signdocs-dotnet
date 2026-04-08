using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record HealthCheckResponse(
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("timestamp")] string? Timestamp,
    [property: JsonPropertyName("services")] Dictionary<string, ServiceStatus>? Services
);

public record ServiceStatus(
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("latency")] double? Latency
);
