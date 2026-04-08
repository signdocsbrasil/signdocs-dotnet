using System.Net;
using System.Net.Http.Headers;
using SignDocsBrasil.Api.Errors;
using SignDocsBrasil.Api.Internal;

namespace SignDocsBrasil.Api.Tests;

public class RetryPolicyTests
{
    [Theory]
    [InlineData(429)]
    [InlineData(500)]
    [InlineData(503)]
    public void IsRetryable_ReturnsTrue_ForRetryableStatusCodes(int statusCode)
    {
        Assert.True(RetryPolicy.IsRetryable(statusCode));
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(204)]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(409)]
    [InlineData(422)]
    public void IsRetryable_ReturnsFalse_ForNonRetryableStatusCodes(int statusCode)
    {
        Assert.False(RetryPolicy.IsRetryable(statusCode));
    }

    [Fact]
    public void CalculateDelay_UsesExponentialBackoff_Attempt0()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        long delay = RetryPolicy.CalculateDelay(0, response);

        // 2^0 * 1000 = 1000, plus jitter 0..999 => [1000, 1999]
        Assert.InRange(delay, 1000, 1999);
    }

    [Fact]
    public void CalculateDelay_UsesExponentialBackoff_Attempt1()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        long delay = RetryPolicy.CalculateDelay(1, response);

        // 2^1 * 1000 = 2000, plus jitter 0..999 => [2000, 2999]
        Assert.InRange(delay, 2000, 2999);
    }

    [Fact]
    public void CalculateDelay_UsesExponentialBackoff_Attempt2()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        long delay = RetryPolicy.CalculateDelay(2, response);

        // 2^2 * 1000 = 4000, plus jitter 0..999 => [4000, 4999]
        Assert.InRange(delay, 4000, 4999);
    }

    [Fact]
    public void CalculateDelay_UsesExponentialBackoff_Attempt3()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        long delay = RetryPolicy.CalculateDelay(3, response);

        // 2^3 * 1000 = 8000, plus jitter 0..999 => [8000, 8999]
        Assert.InRange(delay, 8000, 8999);
    }

    [Fact]
    public void CalculateDelay_CappedAtMaxDelay()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // 2^15 * 1000 = 32768000 > MaxDelayMs (30000), so capped
        long delay = RetryPolicy.CalculateDelay(15, response);

        Assert.True(delay <= RetryPolicy.MaxDelayMs);
    }

    [Fact]
    public void CalculateDelay_RespectsRetryAfterHeader()
    {
        using var response = new HttpResponseMessage((HttpStatusCode)429);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5));

        long delay = RetryPolicy.CalculateDelay(0, response);

        Assert.Equal(5000, delay);
    }

    [Fact]
    public void CalculateDelay_RespectsRetryAfterHeaderRawValue()
    {
        using var response = new HttpResponseMessage((HttpStatusCode)429);
        response.Headers.TryAddWithoutValidation("Retry-After", "3");

        long delay = RetryPolicy.CalculateDelay(0, response);

        // Either parsed as Delta or raw string: 3 seconds = 3000ms
        Assert.Equal(3000, delay);
    }

    [Fact]
    public void CalculateDelay_RetryAfterTakesPriority_OverBackoff()
    {
        using var response = new HttpResponseMessage((HttpStatusCode)429);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));

        // Even though attempt=0 would give ~1000ms, Retry-After says 10s
        long delay = RetryPolicy.CalculateDelay(0, response);

        Assert.Equal(10000, delay);
    }

    [Fact]
    public void CheckTimeout_DoesNotThrow_WhenWithinLimit()
    {
        long startTime = Environment.TickCount64;

        var ex = Record.Exception(() => RetryPolicy.CheckTimeout(startTime));
        Assert.Null(ex);
    }

    [Fact]
    public void CheckTimeout_Throws_WhenExceeded()
    {
        // Simulate a start time 61 seconds ago
        long startTime = Environment.TickCount64 - 61_000;

        Assert.Throws<SignDocsTimeoutException>(() => RetryPolicy.CheckTimeout(startTime));
    }

    [Fact]
    public void CheckTimeout_Throws_AtExactBoundary()
    {
        // Simulate a start time at exactly the max duration + 1ms
        long startTime = Environment.TickCount64 - (RetryPolicy.MaxTotalDurationMs + 1);

        Assert.Throws<SignDocsTimeoutException>(() => RetryPolicy.CheckTimeout(startTime));
    }

    [Fact]
    public void MaxTotalDurationMs_Is60Seconds()
    {
        Assert.Equal(60_000, RetryPolicy.MaxTotalDurationMs);
    }

    [Fact]
    public void MaxDelayMs_Is30Seconds()
    {
        Assert.Equal(30_000, RetryPolicy.MaxDelayMs);
    }

    [Fact]
    public async Task DelayAsync_CompletesQuickly()
    {
        // Just a sanity check that delay runs and completes
        var before = DateTime.UtcNow;
        await RetryPolicy.DelayAsync(10, CancellationToken.None);
        var elapsed = DateTime.UtcNow - before;

        Assert.True(elapsed.TotalMilliseconds >= 5);
    }

    [Fact]
    public async Task DelayAsync_RespectsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => RetryPolicy.DelayAsync(10_000, cts.Token));
    }
}
