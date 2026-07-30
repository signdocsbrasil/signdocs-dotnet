using System.Text.Json.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>Identifies a session stopped by an envelope cancel.</summary>
/// <param name="SessionId">Signing session identifier.</param>
/// <param name="TransactionId">Underlying transaction identifier.</param>
public record CancelledEnvelopeSession(
    [property: JsonPropertyName("sessionId")] string? SessionId,
    [property: JsonPropertyName("transactionId")] string? TransactionId
);

/// <summary>Result of cancelling an entire envelope.</summary>
/// <param name="EnvelopeId">Envelope identifier.</param>
/// <param name="Status">Envelope status after cancellation (CANCELLED).</param>
/// <param name="CancelledCount">
/// How many pending sessions were transitioned to CANCELLED, killing their
/// signing links.
/// </param>
/// <param name="PreservedSignedCount">
/// How many already-collected signatures were left untouched. Cancelling stops
/// the pending signers; it never invalidates evidence already gathered.
/// </param>
/// <param name="CancelledSessions">The sessions that were cancelled.</param>
/// <param name="AlreadyCancelled">
/// True when the envelope was already CANCELLED, in which case
/// <paramref name="CancelledCount"/> is 0. The endpoint is idempotent, so
/// re-cancelling is a safe no-op.
/// </param>
public record CancelEnvelopeResponse(
    [property: JsonPropertyName("envelopeId")] string? EnvelopeId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("cancelledCount")] int CancelledCount,
    [property: JsonPropertyName("preservedSignedCount")] int PreservedSignedCount,
    [property: JsonPropertyName("cancelledSessions")] IReadOnlyList<CancelledEnvelopeSession>? CancelledSessions = null,
    [property: JsonPropertyName("alreadyCancelled")] bool AlreadyCancelled = false
);
