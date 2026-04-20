using System.Globalization;

namespace SignDocsBrasil.Api;

/// <summary>
/// Captures response-level metadata that's typically consumed for observability
/// and lifecycle signaling: IETF <c>RateLimit-*</c> counters, RFC 8594
/// <c>Deprecation</c>/<c>Sunset</c> signaling, and the upstream request ID.
///
/// Exposed via <see cref="SignDocsBrasilClientOptions.OnResponse"/>. The SDK does
/// not otherwise surface these headers to resource methods, so the callback is
/// the single place to plug in observability (logging, metrics, deprecation
/// alerts).
/// </summary>
/// <param name="RateLimitLimit">From <c>RateLimit-Limit</c>.</param>
/// <param name="RateLimitRemaining">From <c>RateLimit-Remaining</c>.</param>
/// <param name="RateLimitReset">From <c>RateLimit-Reset</c> (seconds from now).</param>
/// <param name="Deprecation">Parsed <c>Deprecation</c> header (RFC 8594).</param>
/// <param name="Sunset">Parsed <c>Sunset</c> header (RFC 8594).</param>
/// <param name="RequestId">Upstream <c>X-Request-Id</c> or <c>X-SignDocs-Request-Id</c>.</param>
/// <param name="StatusCode">HTTP status code.</param>
/// <param name="Method">HTTP method (uppercased).</param>
/// <param name="Path">Request path (with query string if any).</param>
public sealed record ResponseMetadata(
    int? RateLimitLimit,
    int? RateLimitRemaining,
    int? RateLimitReset,
    DateTimeOffset? Deprecation,
    DateTimeOffset? Sunset,
    string? RequestId,
    int StatusCode,
    string Method,
    string Path)
{
    /// <summary>
    /// True if the endpoint is marked deprecated (has a <c>Deprecation</c> header).
    /// </summary>
    public bool IsDeprecated() => Deprecation.HasValue;

    /// <summary>
    /// Build a <see cref="ResponseMetadata"/> by parsing the standard observability
    /// headers from <paramref name="response"/>.
    /// </summary>
    public static ResponseMetadata FromResponse(HttpResponseMessage response, string method, string path)
    {
        return new ResponseMetadata(
            RateLimitLimit: ParseIntHeader(response, "RateLimit-Limit"),
            RateLimitRemaining: ParseIntHeader(response, "RateLimit-Remaining"),
            RateLimitReset: ParseIntHeader(response, "RateLimit-Reset"),
            Deprecation: ParseRfc8594Date(GetFirstHeader(response, "Deprecation")),
            Sunset: ParseRfc8594Date(GetFirstHeader(response, "Sunset")),
            RequestId: GetFirstHeader(response, "X-Request-Id")
                       ?? GetFirstHeader(response, "X-SignDocs-Request-Id"),
            StatusCode: (int)response.StatusCode,
            Method: method.ToUpperInvariant(),
            Path: path);
    }

    private static string? GetFirstHeader(HttpResponseMessage response, string name)
    {
        if (response.Headers.TryGetValues(name, out IEnumerable<string>? values))
        {
            string? first = values.FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
            {
                return first;
            }
        }

        if (response.Content is not null
            && response.Content.Headers.TryGetValues(name, out IEnumerable<string>? contentValues))
        {
            string? first = contentValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
            {
                return first;
            }
        }

        return null;
    }

    private static int? ParseIntHeader(HttpResponseMessage response, string name)
    {
        string? raw = GetFirstHeader(response, name);
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
        {
            return parsed;
        }

        return null;
    }

    /// <summary>
    /// Parse an RFC 8594 <c>Deprecation</c> / <c>Sunset</c> header. Accepts either an
    /// IMF-fixdate (HTTP-date, RFC 1123) or an <c>@&lt;unix-seconds&gt;</c> form. Returns
    /// <c>null</c> for any unparseable input.
    /// </summary>
    private static DateTimeOffset? ParseRfc8594Date(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        raw = raw.Trim();

        if (raw.Length > 1 && raw[0] == '@'
            && long.TryParse(raw.AsSpan(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out long unixSeconds))
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        if (DateTimeOffset.TryParseExact(
                raw,
                "r",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTimeOffset exact))
        {
            return exact;
        }

        if (DateTimeOffset.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTimeOffset loose))
        {
            return loose;
        }

        return null;
    }
}
