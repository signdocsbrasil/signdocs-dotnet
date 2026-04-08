using System.Security.Cryptography;
using System.Web;
using SignDocsBrasil.Api.Errors;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class AuthHandlerTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
    {
        foreach (var d in _disposables) d.Dispose();
    }

    [Fact]
    public async Task ClientSecret_AcquiresToken()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "test-secret");
        _disposables.Add(auth);

        handler.EnqueueToken("acquired-token", 900);

        string token = await auth.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("acquired-token", token);
    }

    [Fact]
    public async Task ClientSecret_SendsCorrectFormParams()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "my-secret");
        _disposables.Add(auth);

        handler.EnqueueToken("tok", 900);

        await auth.GetAccessTokenAsync(CancellationToken.None);

        Assert.Single(handler.Requests);
        HttpRequestMessage req = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, req.Method);

        string body = handler.RequestBodies[0]!;
        var parsed = HttpUtility.ParseQueryString(body);
        Assert.Equal("client_credentials", parsed["grant_type"]);
        Assert.Equal("test-client-id", parsed["client_id"]);
        Assert.Equal("my-secret", parsed["client_secret"]);
        Assert.Contains("transactions:read", parsed["scope"]);
    }

    [Fact]
    public async Task ClientSecret_DoesNotSendAssertionParams()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "secret");
        _disposables.Add(auth);

        handler.EnqueueToken("tok", 900);

        await auth.GetAccessTokenAsync(CancellationToken.None);

        string body = handler.RequestBodies[0]!;
        Assert.DoesNotContain("client_assertion", body);
        Assert.DoesNotContain("client_assertion_type", body);
    }

    [Fact]
    public async Task PrivateKeyJwt_SendsAssertionParams()
    {
        // Generate a real EC key for testing
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        string pem = ExportPkcs8Pem(ecdsa);

        var (auth, handler) = TestClientFactory.CreateAuth(
            clientSecret: null,
            privateKeyPem: pem,
            kid: "key-001");
        _disposables.Add(auth);

        handler.EnqueueToken("jwt-token", 900);

        string token = await auth.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("jwt-token", token);
        Assert.Single(handler.Requests);

        string body = handler.RequestBodies[0]!;
        var parsed = HttpUtility.ParseQueryString(body);
        Assert.Equal("urn:ietf:params:oauth:client-assertion-type:jwt-bearer", parsed["client_assertion_type"]);
        Assert.NotNull(parsed["client_assertion"]);
        Assert.Null(parsed["client_secret"]);
    }

    [Fact]
    public async Task PrivateKeyJwt_AssertionIsThreePartJwt()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        string pem = ExportPkcs8Pem(ecdsa);

        var (auth, handler) = TestClientFactory.CreateAuth(
            clientSecret: null,
            privateKeyPem: pem,
            kid: "key-002");
        _disposables.Add(auth);

        handler.EnqueueToken("tok", 900);

        await auth.GetAccessTokenAsync(CancellationToken.None);

        string body = handler.RequestBodies[0]!;
        var parsed = HttpUtility.ParseQueryString(body);
        string assertion = parsed["client_assertion"]!;
        string[] parts = assertion.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public async Task PrivateKeyJwt_JwtHeaderContainsES256AndKid()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        string pem = ExportPkcs8Pem(ecdsa);

        var (auth, handler) = TestClientFactory.CreateAuth(
            clientSecret: null,
            privateKeyPem: pem,
            kid: "key-003");
        _disposables.Add(auth);

        handler.EnqueueToken("tok", 900);

        await auth.GetAccessTokenAsync(CancellationToken.None);

        string body = handler.RequestBodies[0]!;
        var parsed = HttpUtility.ParseQueryString(body);
        string assertion = parsed["client_assertion"]!;
        string headerJson = DecodeBase64Url(assertion.Split('.')[0]);
        Assert.Contains("\"alg\":\"ES256\"", headerJson);
        Assert.Contains("\"kid\":\"key-003\"", headerJson);
    }

    [Fact]
    public async Task TokenCaching_SecondCallReturnsCached()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "secret");
        _disposables.Add(auth);

        handler.EnqueueToken("cached-token", 900);

        string first = await auth.GetAccessTokenAsync(CancellationToken.None);
        string second = await auth.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("cached-token", first);
        Assert.Equal("cached-token", second);
        Assert.Single(handler.Requests); // Only one HTTP call
    }

    [Fact]
    public async Task TokenCaching_RefreshesWhenExpired()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "secret");
        _disposables.Add(auth);

        // Token with 1 second expiry (will be considered expired due to 30s buffer)
        handler.EnqueueToken("token-1", 1);
        handler.EnqueueToken("token-2", 900);

        string first = await auth.GetAccessTokenAsync(CancellationToken.None);
        string second = await auth.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("token-1", first);
        Assert.Equal("token-2", second);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task TokenCaching_DoesNotRefreshWhenStillValid()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "secret");
        _disposables.Add(auth);

        handler.EnqueueToken("long-token", 3600);

        string first = await auth.GetAccessTokenAsync(CancellationToken.None);
        string second = await auth.GetAccessTokenAsync(CancellationToken.None);
        string third = await auth.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("long-token", first);
        Assert.Equal("long-token", second);
        Assert.Equal("long-token", third);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task FailedTokenRequest_ThrowsAuthenticationException()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "secret");
        _disposables.Add(auth);

        handler.EnqueueJson(401, """{"error":"invalid_client"}""");

        await Assert.ThrowsAsync<AuthenticationException>(
            () => auth.GetAccessTokenAsync(CancellationToken.None));
    }

    [Fact]
    public async Task MalformedTokenResponse_ThrowsAuthenticationException()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "secret");
        _disposables.Add(auth);

        handler.EnqueueJson(200, """{"unexpected":"response"}""");

        await Assert.ThrowsAsync<AuthenticationException>(
            () => auth.GetAccessTokenAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ServerError_ThrowsAuthenticationException()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "secret");
        _disposables.Add(auth);

        handler.EnqueueJson(500, """{"error":"server_error"}""");

        await Assert.ThrowsAsync<AuthenticationException>(
            () => auth.GetAccessTokenAsync(CancellationToken.None));
    }

    [Fact]
    public async Task TokenRequest_PostsToTokenUrl()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(
            clientSecret: "secret", baseUrl: "https://custom.example.com");
        _disposables.Add(auth);

        handler.EnqueueToken("tok", 900);

        await auth.GetAccessTokenAsync(CancellationToken.None);

        Assert.Equal("https://custom.example.com/oauth2/token",
            handler.Requests[0].RequestUri!.ToString());
    }

    [Fact]
    public async Task ScopeString_JoinedWithSpaces()
    {
        var (auth, handler) = TestClientFactory.CreateAuth(clientSecret: "secret");
        _disposables.Add(auth);

        handler.EnqueueToken("tok", 900);

        await auth.GetAccessTokenAsync(CancellationToken.None);

        string body = handler.RequestBodies[0]!;
        var parsed = HttpUtility.ParseQueryString(body);
        Assert.Equal("transactions:read transactions:write", parsed["scope"]);
    }

    private static string ExportPkcs8Pem(ECDsa ecdsa)
    {
        byte[] keyBytes = ecdsa.ExportPkcs8PrivateKey();
        string base64 = Convert.ToBase64String(keyBytes);
        return $"-----BEGIN PRIVATE KEY-----\n{base64}\n-----END PRIVATE KEY-----";
    }

    private static string DecodeBase64Url(string base64Url)
    {
        string padded = base64Url.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        byte[] bytes = Convert.FromBase64String(padded);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
