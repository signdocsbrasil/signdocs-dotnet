using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class Policy
{
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    [JsonPropertyName("customSteps")]
    public List<string>? CustomSteps { get; set; }
}
