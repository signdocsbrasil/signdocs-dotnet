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
}
