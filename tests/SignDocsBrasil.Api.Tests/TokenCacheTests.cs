using SignDocsBrasil.Api.TokenCache;

namespace SignDocsBrasil.Api.Tests;

public class TokenCacheTests
{
    [Fact]
    public void InMemoryTokenCache_Miss_ReturnsNull()
    {
        var cache = new InMemoryTokenCache();
        Assert.Null(cache.Get("nonexistent"));
    }

    [Fact]
    public void InMemoryTokenCache_Hit_ReturnsStoredToken()
    {
        var cache = new InMemoryTokenCache();
        var token = new CachedToken("abc", DateTimeOffset.UtcNow.AddMinutes(5));

        cache.Set("k", token);

        CachedToken? got = cache.Get("k");
        Assert.NotNull(got);
        Assert.Equal("abc", got!.AccessToken);
    }

    [Fact]
    public void InMemoryTokenCache_ExpiredEntry_ReturnsNull()
    {
        var cache = new InMemoryTokenCache();
        var expired = new CachedToken("old", DateTimeOffset.UtcNow.AddMinutes(-1));

        cache.Set("k", expired);

        Assert.Null(cache.Get("k"));
    }

    [Fact]
    public void InMemoryTokenCache_Delete_RemovesEntry()
    {
        var cache = new InMemoryTokenCache();
        cache.Set("k", new CachedToken("abc", DateTimeOffset.UtcNow.AddMinutes(5)));

        cache.Delete("k");

        Assert.Null(cache.Get("k"));
    }

    [Fact]
    public void InMemoryTokenCache_Delete_MissingKeyIsNoOp()
    {
        var cache = new InMemoryTokenCache();
        // Does not throw
        cache.Delete("never-set");
    }

    [Fact]
    public void InMemoryTokenCache_SharedAcrossInstances_SameKey()
    {
        // Two distinct AuthHandler-like consumers share one cache instance
        var cache = new InMemoryTokenCache();
        string key = TokenCacheKeys.Derive(
            "client-A",
            "https://api.example.com",
            new[] { "transactions:read" });

        cache.Set(key, new CachedToken("shared-token", DateTimeOffset.UtcNow.AddMinutes(10)));

        // "Other" consumer derives the same key from the same inputs
        string key2 = TokenCacheKeys.Derive(
            "client-A",
            "https://api.example.com",
            new[] { "transactions:read" });
        CachedToken? got = cache.Get(key2);

        Assert.NotNull(got);
        Assert.Equal("shared-token", got!.AccessToken);
    }

    [Fact]
    public void CachedToken_IsExpired_HonorsSkew()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var token = new CachedToken("t", now.AddSeconds(20));

        // 30s skew => considered expired even though raw expiry is 20s away
        Assert.True(token.IsExpired(now, TimeSpan.FromSeconds(30)));
        // 10s skew => not expired
        Assert.False(token.IsExpired(now, TimeSpan.FromSeconds(10)));
        // Zero skew => not expired (raw not yet reached)
        Assert.False(token.IsExpired(now, TimeSpan.Zero));
    }

    [Fact]
    public void TokenCacheKeys_Derive_IsDeterministic()
    {
        string a = TokenCacheKeys.Derive(
            "client-X",
            "https://api.example.com",
            new[] { "a", "b", "c" });
        string b = TokenCacheKeys.Derive(
            "client-X",
            "https://api.example.com",
            new[] { "a", "b", "c" });

        Assert.Equal(a, b);
    }

    [Fact]
    public void TokenCacheKeys_Derive_ScopeOrderIrrelevant()
    {
        string sorted = TokenCacheKeys.Derive(
            "client-X",
            "https://api.example.com",
            new[] { "a", "b", "c" });
        string shuffled = TokenCacheKeys.Derive(
            "client-X",
            "https://api.example.com",
            new[] { "c", "a", "b" });

        Assert.Equal(sorted, shuffled);
    }

    [Fact]
    public void TokenCacheKeys_Derive_TrimsTrailingSlashOnBaseUrl()
    {
        string noSlash = TokenCacheKeys.Derive(
            "client-X",
            "https://api.example.com",
            new[] { "a" });
        string withSlash = TokenCacheKeys.Derive(
            "client-X",
            "https://api.example.com/",
            new[] { "a" });

        Assert.Equal(noSlash, withSlash);
    }

    [Fact]
    public void TokenCacheKeys_Derive_DifferentClientIdsYieldDifferentKeys()
    {
        string a = TokenCacheKeys.Derive("client-A", "https://api.example.com", new[] { "x" });
        string b = TokenCacheKeys.Derive("client-B", "https://api.example.com", new[] { "x" });

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void TokenCacheKeys_Derive_HasCorrectPrefixAndLength()
    {
        string key = TokenCacheKeys.Derive(
            "client-X",
            "https://api.example.com",
            new[] { "x" });

        Assert.StartsWith("signdocs.oauth.", key);
        Assert.Equal(47, key.Length); // 15 prefix + 32 hex
    }

    [Fact]
    public void TokenCacheKeys_Derive_HashIsLowercaseHex()
    {
        string key = TokenCacheKeys.Derive(
            "client-X",
            "https://api.example.com",
            new[] { "x" });

        string hash = key["signdocs.oauth.".Length..];
        Assert.Matches("^[0-9a-f]{32}$", hash);
    }

    [Fact]
    public void TokenCacheKeys_Derive_DoesNotLeakClientId()
    {
        const string secretClientId = "super-secret-client-id-12345";
        string key = TokenCacheKeys.Derive(
            secretClientId,
            "https://api.example.com",
            new[] { "x" });

        Assert.DoesNotContain(secretClientId, key);
    }
}
