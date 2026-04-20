using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.TokenCache;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class AuthHandlerPublicSurfaceTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
    {
        foreach (IDisposable d in _disposables) d.Dispose();
    }

    [Fact]
    public void AuthHandler_IsPublic()
    {
        Type t = typeof(AuthHandler);
        Assert.True(t.IsPublic, "AuthHandler must be public so consumers can plug in their own cache");
    }

    [Fact]
    public void AuthHandler_IsNotSealed()
    {
        Type t = typeof(AuthHandler);
        Assert.False(t.IsSealed, "AuthHandler must not be sealed so consumers can subclass it");
    }

    [Fact]
    public async Task AuthHandler_UsesInjectedCache()
    {
        var spy = new SpyTokenCache();
        var handler = new MockHttpHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        var auth = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: "secret",
            privateKeyPem: null,
            kid: null,
            tokenUrl: "https://api.test.com/oauth2/token",
            scopes: new[] { "transactions:read" },
            testHttpClient: httpClient,
            cache: spy,
            baseUrl: "https://api.test.com");
        _disposables.Add(auth);

        handler.EnqueueToken("injected-token", 900);

        string token = await auth.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("injected-token", token);
        Assert.True(spy.GetCount >= 1, "Get should have been called at least once");
        Assert.Equal(1, spy.SetCount);
    }

    [Fact]
    public async Task AuthHandler_Invalidate_DeletesCacheEntry()
    {
        var spy = new SpyTokenCache();
        var handler = new MockHttpHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        var auth = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: "secret",
            privateKeyPem: null,
            kid: null,
            tokenUrl: "https://api.test.com/oauth2/token",
            scopes: new[] { "transactions:read" },
            testHttpClient: httpClient,
            cache: spy);
        _disposables.Add(auth);

        handler.EnqueueToken("t1", 900);
        await auth.GetAccessTokenAsync(CancellationToken.None);

        auth.Invalidate();

        Assert.Equal(1, spy.DeleteCount);
    }

    [Fact]
    public async Task AuthHandler_SharedCache_ReusesTokenAcrossInstances()
    {
        var shared = new InMemoryTokenCache();
        var handler = new MockHttpHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        // Only enqueue ONE token response — the second handler should hit cache
        handler.EnqueueToken("shared-token", 900);

        var first = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: "secret",
            privateKeyPem: null,
            kid: null,
            tokenUrl: "https://api.test.com/oauth2/token",
            scopes: new[] { "transactions:read" },
            testHttpClient: httpClient,
            cache: shared,
            baseUrl: "https://api.test.com");
        _disposables.Add(first);

        string t1 = await first.GetAccessTokenAsync(CancellationToken.None);

        var second = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: "secret",
            privateKeyPem: null,
            kid: null,
            tokenUrl: "https://api.test.com/oauth2/token",
            scopes: new[] { "transactions:read" },
            testHttpClient: httpClient,
            cache: shared,
            baseUrl: "https://api.test.com");
        _disposables.Add(second);

        string t2 = await second.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("shared-token", t1);
        Assert.Equal("shared-token", t2);
        Assert.Single(handler.Requests); // No second HTTP call
    }

    [Fact]
    public void AuthHandler_CacheKey_HasSignDocsPrefix()
    {
        var auth = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: "secret",
            privateKeyPem: null,
            kid: null,
            tokenUrl: "https://api.test.com/oauth2/token",
            scopes: new[] { "transactions:read" });
        _disposables.Add(auth);

        Assert.StartsWith("signdocs.oauth.", auth.CacheKey);
        Assert.Equal(47, auth.CacheKey.Length);
    }

    private sealed class SpyTokenCache : ITokenCache
    {
        private readonly InMemoryTokenCache _inner = new();

        public int GetCount { get; private set; }
        public int SetCount { get; private set; }
        public int DeleteCount { get; private set; }

        public CachedToken? Get(string key)
        {
            GetCount++;
            return _inner.Get(key);
        }

        public void Set(string key, CachedToken token)
        {
            SetCount++;
            _inner.Set(key, token);
        }

        public void Delete(string key)
        {
            DeleteCount++;
            _inner.Delete(key);
        }
    }
}
