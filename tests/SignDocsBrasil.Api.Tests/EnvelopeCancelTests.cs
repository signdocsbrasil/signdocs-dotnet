using System.Text.Json;
using SignDocsBrasil.Api.Models;
using Xunit;

namespace SignDocsBrasil.Api.Tests;

/// <summary>
/// An envelope is cancelled through its own endpoint. Cancelling the member
/// sessions one by one is not equivalent — it leaves the envelope itself ACTIVE.
/// </summary>
public class EnvelopeCancelTests
{
    [Fact]
    public void DeserializesCancelResult()
    {
        const string json = """
        {
          "envelopeId": "env_1",
          "status": "CANCELLED",
          "cancelledCount": 2,
          "preservedSignedCount": 1,
          "cancelledSessions": [{ "sessionId": "ss_a", "transactionId": "tx_a" }]
        }
        """;

        var resp = JsonSerializer.Deserialize<CancelEnvelopeResponse>(json);

        Assert.NotNull(resp);
        Assert.Equal("env_1", resp!.EnvelopeId);
        Assert.Equal(2, resp.CancelledCount);
        // A signature already collected is never invalidated by cancelling.
        Assert.Equal(1, resp.PreservedSignedCount);
        Assert.Equal("ss_a", resp.CancelledSessions![0].SessionId);
        Assert.False(resp.AlreadyCancelled);
    }

    [Fact]
    public void DeserializesIdempotentReCancel()
    {
        // Re-cancelling is a safe no-op, not an error.
        const string json = """
        {"envelopeId":"env_1","status":"CANCELLED","cancelledCount":0,"alreadyCancelled":true}
        """;

        var resp = JsonSerializer.Deserialize<CancelEnvelopeResponse>(json);

        Assert.NotNull(resp);
        Assert.True(resp!.AlreadyCancelled);
        Assert.Equal(0, resp.CancelledCount);
    }
}
