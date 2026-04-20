namespace SignDocsBrasil.Api.TokenCache;

/// <summary>
/// Pluggable cache for OAuth2 access tokens.
///
/// The default implementation is <see cref="InMemoryTokenCache"/>, which scopes
/// the cache to the lifetime of a single process. Long-lived hosts (console apps,
/// ASP.NET Core services, worker services) can keep using the default.
/// Stateless or short-lived workloads (AWS Lambda, Azure Functions, scale-to-zero
/// containers) SHOULD supply an implementation backed by a shared store
/// (Redis, DynamoDB, Memcached, SQL, etc.) to avoid fetching a fresh token on
/// every invocation.
///
/// Implementations MUST be safe to call concurrently — a <see cref="Set"/> that
/// races with another <see cref="Set"/> for the same key should leave the cache
/// in a consistent state. Implementations SHOULD treat the key as opaque; the SDK
/// derives keys deterministically via
/// <see cref="TokenCacheKeys.Derive(string, string, System.Collections.Generic.IEnumerable{string})"/>.
/// </summary>
public interface ITokenCache
{
    /// <summary>
    /// Retrieve a cached token for <paramref name="key"/>, or <c>null</c> if missing or expired.
    /// Implementations SHOULD return <c>null</c> (not throw) on any backend error.
    /// </summary>
    CachedToken? Get(string key);

    /// <summary>
    /// Store <paramref name="token"/> under <paramref name="key"/>. Implementations SHOULD
    /// honor the token's <see cref="CachedToken.ExpiresAt"/> as the storage TTL upper bound.
    /// </summary>
    void Set(string key, CachedToken token);

    /// <summary>
    /// Remove the cached token for <paramref name="key"/>. Idempotent — deleting a missing
    /// entry is a no-op.
    /// </summary>
    void Delete(string key);
}
