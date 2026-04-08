using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class RegisterWebhookRequest
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("events")]
    public List<string>? Events { get; set; }
}
