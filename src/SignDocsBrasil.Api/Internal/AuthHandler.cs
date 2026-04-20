using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SignDocsBrasil.Api.Errors;
using SignDocsBrasil.Api.TokenCache;

namespace SignDocsBrasil.Api.Internal;

/// <summary>
/// Handles OAuth2 token acquisition and caching for the SignDocsBrasil API.
/// Supports both client_secret and private_key_jwt (ES256) authentication modes.
///
/// Tokens are cached via a pluggable <see cref="ITokenCache"/>. The default
/// <see cref="InMemoryTokenCache"/> preserves the pre-1.3 single-process
/// behavior. Stateless hosts (AWS Lambda, Azure Functions, scale-to-zero
/// containers) should inject a shared-store cache to avoid fetching a fresh
/// token on every invocation.
///
/// <para>
/// Public and non-sealed since 1.3.0. Subclassing is supported, but prefer
/// injecting a custom <see cref="ITokenCache"/> over subclassing for most
/// use cases. The class lives under the <c>SignDocsBrasil.Api.Internal</c>
/// namespace for historical reasons; it is part of the public API surface.
/// </para>
/// Thread-safe via <see cref="SemaphoreSlim"/>.
/// </summary>
public class AuthHandler : IDisposable
{
    private static readonly TimeSpan TokenExpiryBuffer = TimeSpan.FromSeconds(30);

    private readonly string _clientId;
    private readonly string? _clientSecret;
    private readonly string? _privateKeyPem;
    private readonly string? _kid;
    private readonly string _tokenUrl;
    private readonly string _baseUrl;
    private readonly IReadOnlyList<string> _scopes;
    private readonly string _scopeString;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ITokenCache _cache;
    private readonly string _cacheKey;

    /// <summary>
    /// Creates an AuthHandler from SDK options.
    /// Constructs its own internal <see cref="HttpClient"/> with a 10-second timeout.
    /// </summary>
    public AuthHandler(
        string clientId,
        string? clientSecret,
        string? privateKeyPem,
        string? kid,
        string tokenUrl,
        IReadOnlyList<string> scopes,
        ITokenCache? cache = null,
        string? baseUrl = null)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _privateKeyPem = privateKeyPem;
        _kid = kid;
        _tokenUrl = tokenUrl;
        _baseUrl = baseUrl ?? DeriveBaseUrlFromTokenUrl(tokenUrl);
        _scopes = scopes;
        _scopeString = string.Join(" ", scopes);
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _ownsHttpClient = true;
        _cache = cache ?? new InMemoryTokenCache();
        _cacheKey = TokenCacheKeys.Derive(clientId, _baseUrl, scopes);
    }

    /// <summary>
    /// Constructor accepting a test <see cref="HttpClient"/> for unit testing.
    /// The caller retains ownership of the <paramref name="testHttpClient"/>.
    /// </summary>
    public AuthHandler(
        string clientId,
        string? clientSecret,
        string? privateKeyPem,
        string? kid,
        string tokenUrl,
        IReadOnlyList<string> scopes,
        HttpClient testHttpClient,
        ITokenCache? cache = null,
        string? baseUrl = null)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _privateKeyPem = privateKeyPem;
        _kid = kid;
        _tokenUrl = tokenUrl;
        _baseUrl = baseUrl ?? DeriveBaseUrlFromTokenUrl(tokenUrl);
        _scopes = scopes;
        _scopeString = string.Join(" ", scopes);
        _httpClient = testHttpClient;
        _ownsHttpClient = false;
        _cache = cache ?? new InMemoryTokenCache();
        _cacheKey = TokenCacheKeys.Derive(clientId, _baseUrl, scopes);
    }

    /// <summary>
    /// The deterministic cache key this handler uses for reads/writes against
    /// the configured <see cref="ITokenCache"/>.
    /// </summary>
    public string CacheKey => _cacheKey;

    /// <summary>
    /// Returns a valid access token, fetching a new one if the cached token is expired or absent.
    /// Thread-safe: concurrent callers will wait while a token is being refreshed.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A valid Bearer access token.</returns>
    /// <exception cref="AuthenticationException">Thrown when the token request fails.</exception>
    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            CachedToken? cached = _cache.Get(_cacheKey);
            if (cached is not null && !cached.IsExpired(DateTimeOffset.UtcNow, TokenExpiryBuffer))
            {
                return cached.AccessToken;
            }

            return await FetchTokenAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Invalidate the cached token so that the next call to
    /// <see cref="GetAccessTokenAsync"/> will fetch a fresh token from the
    /// authorization server.
    /// </summary>
    public void Invalidate()
    {
        _cache.Delete(_cacheKey);
    }

    private async Task<string> FetchTokenAsync(CancellationToken ct)
    {
        try
        {
            var formFields = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", _clientId),
                new("scope", _scopeString)
            };

            if (!string.IsNullOrEmpty(_clientSecret))
            {
                formFields.Add(new KeyValuePair<string, string>("client_secret", _clientSecret));
            }
            else if (!string.IsNullOrEmpty(_privateKeyPem))
            {
                string assertion = BuildJwtAssertion();
                formFields.Add(new KeyValuePair<string, string>(
                    "client_assertion_type",
                    "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"));
                formFields.Add(new KeyValuePair<string, string>("client_assertion", assertion));
            }

            using var content = new FormUrlEncodedContent(formFields);
            using var request = new HttpRequestMessage(HttpMethod.Post, _tokenUrl) { Content = content };

            using HttpResponseMessage response = await _httpClient
                .SendAsync(request, ct)
                .ConfigureAwait(false);

            string responseBody = await response.Content
                .ReadAsStringAsync(ct)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException(
                    $"Token request failed ({(int)response.StatusCode}): {responseBody}");
            }

            using JsonDocument doc = JsonDocument.Parse(responseBody);
            JsonElement root = doc.RootElement;

            string accessToken = root.GetProperty("access_token").GetString()
                ?? throw new AuthenticationException("Token response missing access_token");
            long expiresIn = root.GetProperty("expires_in").GetInt64();

            var token = new CachedToken(
                AccessToken: accessToken,
                ExpiresAt: DateTimeOffset.UtcNow.AddSeconds(expiresIn));

            _cache.Set(_cacheKey, token);

            return accessToken;
        }
        catch (AuthenticationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AuthenticationException(
                $"Failed to acquire access token: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Builds a JWT assertion for private_key_jwt authentication using ES256.
    /// Uses <see cref="ECDsa.ImportPkcs8PrivateKey"/> and
    /// <see cref="DSASignatureFormat.IeeeP1363FixedFieldConcatenation"/> so no
    /// manual DER-to-raw conversion is needed.
    /// </summary>
    private string BuildJwtAssertion()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var header = new JsonObject
        {
            ["alg"] = "ES256",
            ["typ"] = "JWT",
            ["kid"] = _kid
        };

        var payload = new JsonObject
        {
            ["iss"] = _clientId,
            ["sub"] = _clientId,
            ["aud"] = _tokenUrl,
            ["exp"] = now + 300,
            ["iat"] = now,
            ["jti"] = Guid.NewGuid().ToString()
        };

        string encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(header.ToJsonString()));
        string encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payload.ToJsonString()));
        string signingInput = $"{encodedHeader}.{encodedPayload}";

        using ECDsa ecdsa = ECDsa.Create();
        ecdsa.ImportPkcs8PrivateKey(ParsePemKey(_privateKeyPem!), out _);

        byte[] signatureBytes = ecdsa.SignData(
            Encoding.UTF8.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        string encodedSignature = Base64UrlEncode(signatureBytes);

        return $"{signingInput}.{encodedSignature}";
    }

    /// <summary>
    /// Strips PEM headers/footers and decodes the Base64 content to raw key bytes.
    /// </summary>
    private static byte[] ParsePemKey(string pem)
    {
        string stripped = pem
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("-----BEGIN EC PRIVATE KEY-----", "")
            .Replace("-----END EC PRIVATE KEY-----", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace(" ", "");

        return Convert.FromBase64String(stripped);
    }

    /// <summary>
    /// Encodes binary data as Base64Url (RFC 4648 section 5) without padding.
    /// </summary>
    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    /// Fallback base-URL derivation when only the token URL was supplied.
    /// Strips <c>/oauth2/token</c> if present, otherwise uses the scheme+host.
    /// </summary>
    private static string DeriveBaseUrlFromTokenUrl(string tokenUrl)
    {
        const string suffix = "/oauth2/token";
        if (tokenUrl.EndsWith(suffix, StringComparison.Ordinal))
        {
            return tokenUrl[..^suffix.Length];
        }

        if (Uri.TryCreate(tokenUrl, UriKind.Absolute, out Uri? uri))
        {
            return $"{uri.Scheme}://{uri.Authority}";
        }

        return tokenUrl;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
