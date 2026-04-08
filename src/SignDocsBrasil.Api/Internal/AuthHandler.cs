using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SignDocsBrasil.Api.Errors;

namespace SignDocsBrasil.Api.Internal;

/// <summary>
/// Handles OAuth2 token acquisition and caching for the SignDocsBrasil API.
/// Supports both client_secret and private_key_jwt (ES256) authentication modes.
/// Thread-safe via <see cref="SemaphoreSlim"/>.
/// </summary>
internal sealed class AuthHandler : IDisposable
{
    private const long TokenExpiryBufferSeconds = 30;

    private readonly string _clientId;
    private readonly string? _clientSecret;
    private readonly string? _privateKeyPem;
    private readonly string? _kid;
    private readonly string _tokenUrl;
    private readonly string _scopeString;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private string? _cachedAccessToken;
    private DateTimeOffset _cachedExpiresAt;

    /// <summary>
    /// Creates an AuthHandler from SDK options.
    /// Constructs its own internal <see cref="HttpClient"/> with a 10-second timeout.
    /// </summary>
    internal AuthHandler(
        string clientId,
        string? clientSecret,
        string? privateKeyPem,
        string? kid,
        string tokenUrl,
        IReadOnlyList<string> scopes)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _privateKeyPem = privateKeyPem;
        _kid = kid;
        _tokenUrl = tokenUrl;
        _scopeString = string.Join(" ", scopes);
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _ownsHttpClient = true;
    }

    /// <summary>
    /// Internal constructor accepting a test <see cref="HttpClient"/> for unit testing.
    /// The caller retains ownership of the <paramref name="testHttpClient"/>.
    /// </summary>
    internal AuthHandler(
        string clientId,
        string? clientSecret,
        string? privateKeyPem,
        string? kid,
        string tokenUrl,
        IReadOnlyList<string> scopes,
        HttpClient testHttpClient)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _privateKeyPem = privateKeyPem;
        _kid = kid;
        _tokenUrl = tokenUrl;
        _scopeString = string.Join(" ", scopes);
        _httpClient = testHttpClient;
        _ownsHttpClient = false;
    }

    /// <summary>
    /// Returns a valid access token, fetching a new one if the cached token is expired or absent.
    /// Thread-safe: concurrent callers will wait while a token is being refreshed.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A valid Bearer access token.</returns>
    /// <exception cref="AuthenticationException">Thrown when the token request fails.</exception>
    internal async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_cachedAccessToken is not null
                && DateTimeOffset.UtcNow < _cachedExpiresAt.AddSeconds(-TokenExpiryBufferSeconds))
            {
                return _cachedAccessToken;
            }

            return await FetchTokenAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
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

            _cachedAccessToken = accessToken;
            _cachedExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

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

        // Build JWT header
        var header = new JsonObject
        {
            ["alg"] = "ES256",
            ["typ"] = "JWT",
            ["kid"] = _kid
        };

        // Build JWT payload
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

        // Sign with ES256 (ECDSA using P-256 and SHA-256)
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

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        _semaphore.Dispose();
    }
}
