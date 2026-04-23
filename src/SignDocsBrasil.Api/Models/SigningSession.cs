using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// A signing session returned by the API when created.
/// </summary>
/// <param name="SessionId">Unique identifier of the session.</param>
/// <param name="TransactionId">Identifier of the underlying transaction.</param>
/// <param name="Status">Session status: ACTIVE, COMPLETED, CANCELLED, EXPIRED, FAILED.</param>
/// <param name="Url">URL of the hosted signing page.</param>
/// <param name="ClientSecret">
/// Session token for widget / redirect authentication.
/// Format: <c>ss_secret_</c> followed by a JWT.
/// </param>
/// <param name="ExpiresAt">Session expiration timestamp (ISO 8601 UTC).</param>
/// <param name="CreatedAt">Session creation timestamp (ISO 8601 UTC).</param>
/// <param name="InviteSent">
/// <c>true</c> when SignDocs dispatched an invitation email to
/// <c>signer.email</c> at session creation. Populated only when
/// <c>owner</c> was provided and <c>signer.email</c> differs from
/// <c>owner.email</c>.
/// </param>
public record SigningSession(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("clientSecret")] string? ClientSecret,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt,
    [property: JsonPropertyName("createdAt")] string? CreatedAt,
    [property: JsonPropertyName("inviteSent")] bool? InviteSent = null
);
