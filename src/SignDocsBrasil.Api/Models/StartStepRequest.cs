using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

public class StartStepRequest
{
    [JsonPropertyName("captureMode")]
    public string? CaptureMode { get; set; }

    [JsonPropertyName("otpChannel")]
    public string? OtpChannel { get; set; }
}
