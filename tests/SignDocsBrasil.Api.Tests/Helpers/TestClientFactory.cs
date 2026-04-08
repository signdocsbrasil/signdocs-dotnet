using SignDocsBrasil.Api.Internal;

namespace SignDocsBrasil.Api.Tests.Helpers;

/// <summary>
/// Creates test instances of SignDocsHttpClient and AuthHandler
/// backed by MockHttpHandler for deterministic testing.
/// </summary>
internal static class TestClientFactory
{
    /// <summary>
    /// Creates a full SignDocsHttpClient with a mock auth handler that uses client_secret flow.
    /// Both the auth HTTP traffic and the API HTTP traffic go through the same MockHttpHandler.
    /// Callers must enqueue a token response before any authenticated API call.
    /// </summary>
    internal static (SignDocsHttpClient client, MockHttpHandler handler) Create(
        int maxRetries = 0,
        string baseUrl = "https://api.test.com")
    {
        var handler = new MockHttpHandler();
        var authHttpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

        var auth = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: "test-secret",
            privateKeyPem: null,
            kid: null,
            tokenUrl: $"{baseUrl}/oauth2/token",
            scopes: new[] { "transactions:read", "transactions:write" },
            testHttpClient: authHttpClient);

        var apiHttpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

        var client = new SignDocsHttpClient(
            apiHttpClient,
            baseUrl,
            TimeSpan.FromSeconds(30),
            auth,
            maxRetries,
            logger: null);

        return (client, handler);
    }

    /// <summary>
    /// Creates an AuthHandler backed by a mock HTTP handler for auth-specific tests.
    /// </summary>
    internal static (AuthHandler auth, MockHttpHandler handler) CreateAuth(
        string? clientSecret = "test-secret",
        string? privateKeyPem = null,
        string? kid = null,
        string baseUrl = "https://api.test.com")
    {
        var handler = new MockHttpHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

        var auth = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: clientSecret,
            privateKeyPem: privateKeyPem,
            kid: kid,
            tokenUrl: $"{baseUrl}/oauth2/token",
            scopes: new[] { "transactions:read", "transactions:write" },
            testHttpClient: httpClient);

        return (auth, handler);
    }
}
