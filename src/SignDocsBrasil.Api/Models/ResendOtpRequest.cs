using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public record ResendOtpRequest(
    [property: JsonPropertyName("channel")] string? Channel = null
);
