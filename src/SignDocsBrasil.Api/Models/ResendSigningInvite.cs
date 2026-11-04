using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Request to resend a session's invite, optionally selecting the channels.
/// </summary>
/// <param name="DeliverVia">
/// Channels for this resend: <c>email</c>, <c>whatsapp</c> and/or <c>telegram</c>.
/// <c>null</c> uses the channels the signer was created with, or e-mail for sessions
/// with none recorded.
/// </param>
public record ResendSigningInviteRequest(
    [property: JsonPropertyName("deliverVia")] List<string>? DeliverVia = null
);

/// <summary>
/// The result of resending a session's invite.
/// </summary>
/// <param name="SessionId">Signing session identifier.</param>
/// <param name="TransactionId">Underlying transaction identifier.</param>
/// <param name="ResentAt">When the invite was resent (ISO 8601 UTC).</param>
/// <param name="To">E-mail the invite was resent to. Set only when the resend included e-mail.</param>
/// <param name="WhatsAppInviteSent">
/// <c>true</c> when Meta accepted the WhatsApp message, which does not confirm delivery.
/// <c>null</c> otherwise.
/// </param>
/// <param name="TelegramInviteSent">
/// Result of the Telegram send, set whenever the resend included Telegram.
/// <c>false</c> means the invite did not arrive.
/// </param>
public record ResendSigningInviteResponse(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("resentAt")] string? ResentAt,
    [property: JsonPropertyName("to")] string? To = null,
    [property: JsonPropertyName("whatsappInviteSent")] bool? WhatsAppInviteSent = null,
    [property: JsonPropertyName("telegramInviteSent")] bool? TelegramInviteSent = null
);
