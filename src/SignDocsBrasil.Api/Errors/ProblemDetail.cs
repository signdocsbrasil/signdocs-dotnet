using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Errors;

public record ProblemDetail(string? Type, string? Title, int Status, string? Detail, string? Instance)
{
    /// <summary>Stable machine-readable case, when the API gives one (e.g. <c>TIMESTAMP_UNAVAILABLE</c>, <c>SIGNER_TURN</c>).</summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>True when the API states that resending the same request is safe.</summary>
    [JsonPropertyName("retryable")]
    public bool? Retryable { get; init; }

    public static ProblemDetail Fallback(int status, string? body) => new(
        $"https://api.signdocs.com.br/errors/{status}", $"HTTP {status}", status, body, null);
}
