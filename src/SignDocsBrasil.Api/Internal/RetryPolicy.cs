using System.Net.Http.Headers;
using SignDocsBrasil.Api.Errors;

namespace SignDocsBrasil.Api.Internal;

/// <summary>
/// Implements exponential backoff retry logic for retryable HTTP status codes.
/// Retries on 429 (Too Many Requests), 500 (Internal Server Error), and 503 (Service Unavailable).
/// Respects the Retry-After header when present on 429 responses.
/// </summary>
internal static class RetryPolicy
{
    private static readonly HashSet<int> RetryableStatusCodes = new() { 429, 500, 503 };

    internal const long MaxTotalDurationMs = 60_000;
    internal const long MaxDelayMs = 30_000;

    [ThreadStatic]
    private static Random? t_random;

    private static Random Random => t_random ??= new Random();

    /// <summary>
    /// Determines whether a given HTTP status code is retryable.
    /// </summary>
    internal static bool IsRetryable(int statusCode) =>
        RetryableStatusCodes.Contains(statusCode);

    /// <summary>
    /// Calculates the delay before the next retry attempt.
    /// Uses the Retry-After header if present (for 429 responses), otherwise
    /// uses exponential backoff with jitter: 2^attempt * 1000 + random(0..999) ms,
    /// capped at <see cref="MaxDelayMs"/>.
    /// </summary>
    /// <param name="attempt">The zero-based attempt number.</param>
    /// <param name="response">The HTTP response from the failed request.</param>
    /// <returns>The delay in milliseconds.</returns>
    internal static long CalculateDelay(int attempt, HttpResponseMessage response)
    {
        // Check for Retry-After header (integer seconds form only)
        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is not null)
        {
            return (long)retryAfter.Delta.Value.TotalMilliseconds;
        }

        // Also handle raw header value if strongly-typed parsing missed it
        if (response.Headers.TryGetValues("Retry-After", out IEnumerable<string>? values))
        {
            string? raw = values.FirstOrDefault();
            if (raw is not null && long.TryParse(raw, out long retryAfterSeconds))
            {
                return retryAfterSeconds * 1000;
            }
        }

        // Exponential backoff with jitter
        long baseDelay = (long)Math.Pow(2, attempt) * 1000;
        long jitter = Random.NextInt64(1000);
        return Math.Min(baseDelay + jitter, MaxDelayMs);
    }

    /// <summary>
    /// Checks whether the total elapsed time has exceeded the maximum allowed duration.
    /// </summary>
    /// <param name="startTimeMs">The start time in epoch milliseconds (<see cref="Environment.TickCount64"/>).</param>
    /// <exception cref="SignDocsTimeoutException">
    /// Thrown when the maximum retry duration of 60 seconds has been exceeded.
    /// </exception>
    internal static void CheckTimeout(long startTimeMs)
    {
        if (Environment.TickCount64 - startTimeMs > MaxTotalDurationMs)
        {
            throw new SignDocsTimeoutException("Request exceeded maximum retry duration of 60s");
        }
    }

    /// <summary>
    /// Asynchronously delays for the specified duration, respecting cancellation.
    /// </summary>
    /// <param name="millis">The number of milliseconds to delay.</param>
    /// <param name="ct">A cancellation token that can cancel the delay.</param>
    internal static async Task DelayAsync(long millis, CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(millis), ct).ConfigureAwait(false);
    }
}
