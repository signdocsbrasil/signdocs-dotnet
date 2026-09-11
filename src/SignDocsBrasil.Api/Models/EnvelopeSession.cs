using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Signing session added to an envelope.
/// </summary>
/// <param name="SessionId">Unique identifier of the session.</param>
/// <param name="TransactionId">Identifier of the underlying transaction.</param>
/// <param name="SignerIndex">Signer position within the envelope.</param>
/// <param name="Status">Session status.</param>
/// <param name="Url">URL of the hosted signing page.</param>
/// <param name="ClientSecret">Session token for widget / redirect authentication.</param>
/// <param name="ExpiresAt">Session expiration timestamp (ISO 8601 UTC).</param>
/// <param name="InviteSent">
/// <c>true</c> when SignDocs dispatched an invitation email to the signer
/// at the time this session was added. Populated only when the envelope
/// was created with an <c>owner</c> and the signer's email differs from
/// <c>owner.email</c>.
/// </param>
/// <param name="WhatsAppInviteSent">
/// <c>true</c> when Meta accepted the WhatsApp message carrying the link —
/// accepted, not delivered. <c>null</c> otherwise.
/// </param>
/// <param name="TelegramInviteSent">
/// Result of the Telegram delivery, set whenever <c>deliverVia</c> included
/// <c>telegram</c>. <c>false</c> means the link did not reach the signer over
/// Telegram (no CPF registered with the bot, or the send failed).
/// </param>
public record EnvelopeSession(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("signerIndex")] int SignerIndex,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("clientSecret")] string? ClientSecret,
    [property: JsonPropertyName("expiresAt")] string? ExpiresAt,
    [property: JsonPropertyName("inviteSent")] bool? InviteSent = null,
    [property: JsonPropertyName("whatsappInviteSent")] bool? WhatsAppInviteSent = null,
    [property: JsonPropertyName("telegramInviteSent")] bool? TelegramInviteSent = null
);
