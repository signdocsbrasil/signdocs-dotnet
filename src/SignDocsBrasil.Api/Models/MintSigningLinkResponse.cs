using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// A freshly minted signing URL for an existing session.
/// </summary>
/// <param name="SessionId">Signing session identifier.</param>
/// <param name="TransactionId">Underlying transaction identifier.</param>
/// <param name="Url">Single-use signing URL. Treat it as a bearer credential.</param>
/// <param name="ExpiresAt">Deadline of the original session; minting does not extend it.</param>
/// <param name="ExpiresIn">Seconds remaining until <paramref name="ExpiresAt"/>.</param>
public record MintSigningLinkResponse(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn
);
