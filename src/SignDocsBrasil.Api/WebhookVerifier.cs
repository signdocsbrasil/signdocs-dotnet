using System.Security.Cryptography;
using System.Text;

namespace SignDocsBrasil.Api;

/// <summary>
/// Utility for verifying webhook signatures.
/// Uses HMAC-SHA256 with constant-time comparison to prevent timing attacks.
/// </summary>
public static class WebhookVerifier
{
    /// <summary>
    /// Default tolerance for timestamp validation: 5 minutes.
    /// </summary>
    public const int DefaultToleranceSeconds = 300;

    /// <summary>
    /// Verifies a webhook signature with the default tolerance of 5 minutes.
    /// </summary>
    public static bool VerifySignature(string body, string signatureHeader, string timestampHeader, string secret)
    {
        return VerifySignature(body, signatureHeader, timestampHeader, secret, DefaultToleranceSeconds);
    }

    /// <summary>
    /// Verifies a webhook signature with a custom tolerance.
    /// </summary>
    /// <param name="body">The raw request body as a string.</param>
    /// <param name="signatureHeader">The value of the X-Signature header (hex-encoded HMAC).</param>
    /// <param name="timestampHeader">The value of the X-Timestamp header (Unix epoch seconds).</param>
    /// <param name="secret">The webhook signing secret.</param>
    /// <param name="toleranceSeconds">The maximum allowed age of the timestamp in seconds.</param>
    /// <returns>True if the signature is valid and the timestamp is within tolerance.</returns>
    public static bool VerifySignature(string? body, string? signatureHeader, string? timestampHeader,
        string? secret, int toleranceSeconds)
    {
        if (body is null || signatureHeader is null || timestampHeader is null || secret is null)
        {
            return false;
        }

        if (!long.TryParse(timestampHeader, out long timestamp))
        {
            return false;
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - timestamp) > toleranceSeconds)
        {
            return false;
        }

        string signingInput = timestamp + "." + body;
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
        byte[] inputBytes = Encoding.UTF8.GetBytes(signingInput);

        byte[] hash = HMACSHA256.HashData(keyBytes, inputBytes);
        string expected = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signatureHeader),
            Encoding.UTF8.GetBytes(expected));
    }
}
