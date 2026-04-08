using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record HealthHistoryResponse(
    [property: JsonPropertyName("entries")] List<HealthCheckResponse>? Entries
);
