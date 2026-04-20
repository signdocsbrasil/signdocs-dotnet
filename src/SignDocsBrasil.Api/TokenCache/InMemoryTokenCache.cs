using System.Collections.Concurrent;

namespace SignDocsBrasil.Api.TokenCache;

/// <summary>
/// Default in-process token cache. Equivalent to the behavior the SDK
/// shipped with in 1.2.x and earlier — cache lives for the lifetime of
/// the process. Thread-safe via <see cref="ConcurrentDictionary{TKey, TValue}"/>.
/// </summary>
public sealed class InMemoryTokenCache : ITokenCache
{
    private readonly ConcurrentDictionary<string, CachedToken> _store = new();

    /// <inheritdoc />
    public CachedToken? Get(string key)
    {
        if (!_store.TryGetValue(key, out CachedToken? entry))
        {
            return null;
        }

        // Drop stale entries eagerly (no skew applied here; callers apply skew).
        if (entry.IsExpired(DateTimeOffset.UtcNow, TimeSpan.Zero))
        {
            _store.TryRemove(key, out _);
            return null;
        }

        return entry;
    }

    /// <inheritdoc />
    public void Set(string key, CachedToken token)
    {
        _store[key] = token;
    }

    /// <inheritdoc />
    public void Delete(string key)
    {
        _store.TryRemove(key, out _);
    }
}
