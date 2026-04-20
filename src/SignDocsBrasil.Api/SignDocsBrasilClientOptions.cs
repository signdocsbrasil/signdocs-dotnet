using Microsoft.Extensions.Logging;
using SignDocsBrasil.Api.TokenCache;

namespace SignDocsBrasil.Api;

public sealed class SignDocsBrasilClientOptions
{
    public static readonly string DefaultBaseUrl = "https://api.signdocs.com.br";
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
    public static readonly int DefaultMaxRetries = 5;
    public static readonly string[] DefaultScopes =
    [
        "transactions:read",
        "transactions:write",
        "steps:write",
        "evidence:read",
        "webhooks:write"
    ];

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? PrivateKey { get; set; }
    public string? Kid { get; set; }
    public string BaseUrl { get; set; } = DefaultBaseUrl;
    public TimeSpan Timeout { get; set; } = DefaultTimeout;
    public int MaxRetries { get; set; } = DefaultMaxRetries;
    public string[] Scopes { get; set; } = DefaultScopes;
    public HttpClient? HttpClient { get; set; }
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Pluggable cache for OAuth2 access tokens. Defaults to an in-process
    /// <see cref="InMemoryTokenCache"/> (1.2.x behavior). Stateless hosts
    /// (AWS Lambda, Azure Functions) should supply a shared-store
    /// implementation backed by Redis, DynamoDB, etc.
    /// </summary>
    public ITokenCache? TokenCache { get; set; }

    /// <summary>
    /// Invoked once per API response with observability data —
    /// <c>RateLimit-*</c> counters, RFC 8594 <c>Deprecation</c> / <c>Sunset</c>,
    /// request ID. Callback exceptions are swallowed and logged (if a logger is
    /// configured); they never fail the surrounding request.
    /// </summary>
    public Action<ResponseMetadata>? OnResponse { get; set; }

    public string TokenUrl => BaseUrl + "/oauth2/token";

    public bool UsesClientSecret => !string.IsNullOrEmpty(ClientSecret);

    public bool UsesPrivateKeyJwt => !string.IsNullOrEmpty(PrivateKey);

    internal void Validate()
    {
        if (string.IsNullOrEmpty(ClientId))
            throw new ArgumentException("ClientId is required.", nameof(ClientId));

        if (!UsesClientSecret && !UsesPrivateKeyJwt)
            throw new ArgumentException(
                "Either ClientSecret or PrivateKey must be provided for authentication.");

        if (UsesPrivateKeyJwt && string.IsNullOrEmpty(Kid))
            throw new ArgumentException(
                "Kid is required when using PrivateKey authentication.", nameof(Kid));
    }
}
