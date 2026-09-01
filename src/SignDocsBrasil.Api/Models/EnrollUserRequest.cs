using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class EnrollUserRequest
{
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    // Inspect without writing. Returns the same verdict the batch endpoint
    // gives and persists nothing — no image, no record, and the 90-day
    // retention clock never starts.
    [JsonPropertyName("dryRun")]
    public bool? DryRun { get; set; }
}
