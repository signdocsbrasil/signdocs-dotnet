using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Tests;

public class WebhookEventTests
{
    // Canonical events from openapi/openapi.yaml WebhookEventType enum
    private static readonly string[] ExpectedWireValues =
    {
        "TRANSACTION.CREATED",
        "TRANSACTION.COMPLETED",
        "TRANSACTION.CANCELLED",
        "TRANSACTION.FAILED",
        "TRANSACTION.EXPIRED",
        "TRANSACTION.FALLBACK",
        "TRANSACTION.DEADLINE_APPROACHING",
        "STEP.STARTED",
        "STEP.COMPLETED",
        "STEP.FAILED",
        "STEP.PURPOSE_DISCLOSURE_SENT",
        "QUOTA.WARNING",
        "API.DEPRECATION_NOTICE",
        "SIGNING_SESSION.CREATED",
        "SIGNING_SESSION.COMPLETED",
        "SIGNING_SESSION.CANCELLED",
        "SIGNING_SESSION.EXPIRED",
        "ENVELOPE.CREATED",
        "ENVELOPE.ALL_SIGNED",
        "ENVELOPE.EXPIRED",
    };

    [Fact]
    public void WebhookEvent_HasAllCanonicalEvents()
    {
        Array values = Enum.GetValues(typeof(WebhookEvent));
        Assert.Equal(20, values.Length);
    }

    [Fact]
    public void WebhookEvent_AllWireValuesMatchSpec()
    {
        HashSet<string> actual = Enum.GetValues(typeof(WebhookEvent))
            .Cast<WebhookEvent>()
            .Select(e => e.ToWireValue())
            .ToHashSet();

        Assert.Equal(ExpectedWireValues.ToHashSet(), actual);
    }

    [Theory]
    [InlineData(WebhookEvent.TransactionCreated, "TRANSACTION.CREATED")]
    [InlineData(WebhookEvent.TransactionDeadlineApproaching, "TRANSACTION.DEADLINE_APPROACHING")]
    [InlineData(WebhookEvent.StepPurposeDisclosureSent, "STEP.PURPOSE_DISCLOSURE_SENT")]
    [InlineData(WebhookEvent.ApiDeprecationNotice, "API.DEPRECATION_NOTICE")]
    public void ToWireValue_ReturnsExpectedString(WebhookEvent ev, string expected)
    {
        Assert.Equal(expected, ev.ToWireValue());
    }

    [Fact]
    public void IsNt65_FlagsTransactionDeadlineApproaching()
    {
        Assert.True(WebhookEvent.TransactionDeadlineApproaching.IsNt65());
    }

    [Fact]
    public void IsNt65_FlagsStepPurposeDisclosureSent()
    {
        Assert.True(WebhookEvent.StepPurposeDisclosureSent.IsNt65());
    }

    [Fact]
    public void IsNt65_FalseForAllNonNt65Events()
    {
        WebhookEvent[] nonNt65 = Enum.GetValues(typeof(WebhookEvent))
            .Cast<WebhookEvent>()
            .Where(e => e != WebhookEvent.TransactionDeadlineApproaching
                        && e != WebhookEvent.StepPurposeDisclosureSent)
            .ToArray();

        foreach (WebhookEvent ev in nonNt65)
        {
            Assert.False(ev.IsNt65(), $"{ev} should not be NT65");
        }
    }

    [Fact]
    public void IsNt65_OnlyTwoEventsAreNt65()
    {
        int nt65Count = Enum.GetValues(typeof(WebhookEvent))
            .Cast<WebhookEvent>()
            .Count(e => e.IsNt65());

        Assert.Equal(2, nt65Count);
    }
}
