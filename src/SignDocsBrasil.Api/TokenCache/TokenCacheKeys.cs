using System.Security.Cryptography;
using System.Text;

namespace SignDocsBrasil.Api.TokenCache;

/// <summary>
/// Helpers for deriving deterministic cache keys from OAuth2 credentials.
///
/// Keys are hashed (SHA-256, truncated to 32 lowercase hex chars) so that
/// a leaked cache key cannot be reversed to recover the client ID. The
/// <c>signdocs.oauth.</c> prefix keeps the key namespaced when shared with
/// other cache consumers.
/// </summary>
public static class TokenCacheKeys
{
    private const string Prefix = "signdocs.oauth.";

    /// <summary>
    /// Derive a cache key from <paramref name="clientId"/>, <paramref name="baseUrl"/>,
    /// and <paramref name="scopes"/>. Scopes are sorted and space-joined so that the
    /// same logical credentials always produce the same key regardless of input order.
    /// Trailing slashes on <paramref name="baseUrl"/> are trimmed.
    /// </summary>
    /// <returns>
    /// A 47-character key of the form <c>signdocs.oauth.&lt;32 lowercase hex chars&gt;</c>.
    /// </returns>
    public static string Derive(string clientId, string baseUrl, IEnumerable<string> scopes)
    {
        string[] canonicalScopes = scopes.ToArray();
        Array.Sort(canonicalScopes, StringComparer.Ordinal);

        string material = $"{clientId}|{baseUrl.TrimEnd('/')}|{string.Join(" ", canonicalScopes)}";

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        string hex = Convert.ToHexString(hash).ToLowerInvariant();

        return Prefix + hex[..32];
    }
}
