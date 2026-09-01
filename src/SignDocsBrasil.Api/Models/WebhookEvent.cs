using System.Runtime.Serialization;

namespace SignDocsBrasil.Api.Models;

/// <summary>
/// Canonical set of webhook event types accepted by the SignDocs API.
///
/// Stays in lockstep with the OpenAPI spec <c>WebhookEventType</c> enum at
/// <c>openapi/openapi.yaml</c>. Events flagged by
/// <see cref="WebhookEventExtensions.IsNt65"/> are emitted only for tenants with
/// <c>nt65ComplianceEnabled</c> (INSS consignado workflow — NT65 / ITI Technical
/// Note 65).
/// </summary>
public enum WebhookEvent
{
    [EnumMember(Value = "TRANSACTION.CREATED")]
    TransactionCreated,

    [EnumMember(Value = "TRANSACTION.COMPLETED")]
    TransactionCompleted,

    [EnumMember(Value = "TRANSACTION.CANCELLED")]
    TransactionCancelled,

    [EnumMember(Value = "TRANSACTION.FAILED")]
    TransactionFailed,

    [EnumMember(Value = "TRANSACTION.EXPIRED")]
    TransactionExpired,

    [EnumMember(Value = "TRANSACTION.FALLBACK")]
    TransactionFallback,

    /// <summary>(NT65) ≤2 business days remaining until INSS submission deadline.</summary>
    [EnumMember(Value = "TRANSACTION.DEADLINE_APPROACHING")]
    TransactionDeadlineApproaching,

    [EnumMember(Value = "STEP.STARTED")]
    StepStarted,

    [EnumMember(Value = "STEP.COMPLETED")]
    StepCompleted,

    [EnumMember(Value = "STEP.FAILED")]
    StepFailed,

    /// <summary>(NT65) Purpose-disclosure notification delivered to the beneficiary.</summary>
    [EnumMember(Value = "STEP.PURPOSE_DISCLOSURE_SENT")]
    StepPurposeDisclosureSent,

    [EnumMember(Value = "ENROLLMENT.EXPIRING")]
    EnrollmentExpiring,

    [EnumMember(Value = "ENROLLMENT.EXPIRED")]
    EnrollmentExpired,

    [EnumMember(Value = "QUOTA.WARNING")]
    QuotaWarning,

    [EnumMember(Value = "API.DEPRECATION_NOTICE")]
    ApiDeprecationNotice,

    [EnumMember(Value = "SIGNING_SESSION.CREATED")]
    SigningSessionCreated,

    [EnumMember(Value = "SIGNING_SESSION.COMPLETED")]
    SigningSessionCompleted,

    [EnumMember(Value = "SIGNING_SESSION.CANCELLED")]
    SigningSessionCancelled,

    [EnumMember(Value = "SIGNING_SESSION.EXPIRED")]
    SigningSessionExpired,

    [EnumMember(Value = "ENVELOPE.CREATED")]
    EnvelopeCreated,

    [EnumMember(Value = "ENVELOPE.ALL_SIGNED")]
    EnvelopeAllSigned,

    [EnumMember(Value = "ENVELOPE.EXPIRED")]
    EnvelopeExpired,
}

/// <summary>
/// Extension helpers for <see cref="WebhookEvent"/>.
/// </summary>
public static class WebhookEventExtensions
{
    /// <summary>
    /// Returns the canonical wire value for this event (e.g. <c>TRANSACTION.CREATED</c>).
    /// This is the exact string the API emits and accepts in webhook registrations.
    /// </summary>
    public static string ToWireValue(this WebhookEvent ev)
    {
        System.Reflection.FieldInfo? field = typeof(WebhookEvent).GetField(ev.ToString());
        if (field is null)
        {
            return ev.ToString();
        }

        EnumMemberAttribute? attr = (EnumMemberAttribute?)Attribute.GetCustomAttribute(
            field, typeof(EnumMemberAttribute));
        return attr?.Value ?? ev.ToString();
    }

    /// <summary>
    /// Returns <c>true</c> for events that belong to the NT65 INSS consignado flow and
    /// are only emitted for tenants with <c>nt65ComplianceEnabled</c>.
    /// </summary>
    public static bool IsNt65(this WebhookEvent ev) => ev switch
    {
        WebhookEvent.TransactionDeadlineApproaching => true,
        WebhookEvent.StepPurposeDisclosureSent => true,
        _ => false,
    };
}
