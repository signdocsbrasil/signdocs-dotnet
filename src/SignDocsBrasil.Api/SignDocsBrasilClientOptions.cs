using Microsoft.Extensions.Logging;

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
