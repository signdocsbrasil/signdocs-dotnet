using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Resources;
using SignDocsBrasil.Api.TokenCache;

namespace SignDocsBrasil.Api;

/// <summary>
/// Main entry point for the SignDocsBrasil API SDK.
/// Thread-safe — a single instance should be reused for the lifetime of the application.
/// </summary>
public sealed class SignDocsBrasilClient : IDisposable
{
    /// <summary>Health check operations (no authentication required).</summary>
    public HealthResource Health { get; }

    /// <summary>Transaction CRUD operations.</summary>
    public TransactionsResource Transactions { get; }

    /// <summary>Document upload, presign, confirm, and download operations.</summary>
    public DocumentsResource Documents { get; }

    /// <summary>Step start and complete operations within a transaction.</summary>
    public StepsResource Steps { get; }

    /// <summary>Digital certificate signing operations (prepare and complete).</summary>
    public SigningResource Signing { get; }

    /// <summary>Evidence retrieval operations.</summary>
    public EvidenceResource Evidence { get; }

    /// <summary>Public verification operations (no authentication required).</summary>
    public VerificationResource Verification { get; }

    /// <summary>User enrollment operations.</summary>
    public UsersResource Users { get; }

    /// <summary>Webhook management operations.</summary>
    public WebhooksResource Webhooks { get; }

    /// <summary>Document group operations.</summary>
    public DocumentGroupsResource DocumentGroups { get; }

    /// <summary>Signing session operations.</summary>
    public SigningSessionsResource SigningSessions { get; }

    /// <summary>Envelope operations for multi-signer document signing.</summary>
    public EnvelopesResource Envelopes { get; }

    private readonly AuthHandler _auth;
    private readonly SignDocsHttpClient _http;

    private SignDocsBrasilClient(SignDocsBrasilClientOptions options)
    {
        options.Validate();

        _auth = new AuthHandler(
            clientId: options.ClientId!,
            clientSecret: options.ClientSecret,
            privateKeyPem: options.PrivateKey,
            kid: options.Kid,
            tokenUrl: options.TokenUrl,
            scopes: options.Scopes,
            cache: options.TokenCache,
            baseUrl: options.BaseUrl);
        _http = new SignDocsHttpClient(
            options.HttpClient,
            options.BaseUrl,
            options.Timeout,
            _auth,
            options.MaxRetries,
            options.Logger,
            options.OnResponse);

        Health = new HealthResource(_http);
        Transactions = new TransactionsResource(_http);
        Documents = new DocumentsResource(_http);
        Steps = new StepsResource(_http);
        Signing = new SigningResource(_http);
        Evidence = new EvidenceResource(_http);
        Verification = new VerificationResource(_http);
        Users = new UsersResource(_http);
        Webhooks = new WebhooksResource(_http);
        DocumentGroups = new DocumentGroupsResource(_http);
        SigningSessions = new SigningSessionsResource(_http);
        Envelopes = new EnvelopesResource(_http);
    }

    /// <summary>
    /// Creates a new builder for constructing a <see cref="SignDocsBrasilClient"/>.
    /// </summary>
    public static Builder CreateBuilder() => new();

    /// <inheritdoc />
    public void Dispose()
    {
        _http.Dispose();
        _auth.Dispose();
    }

    /// <summary>
    /// Builder for constructing a <see cref="SignDocsBrasilClient"/> instance.
    /// </summary>
    public sealed class Builder
    {
        private readonly SignDocsBrasilClientOptions _options = new();

        internal Builder() { }

        /// <summary>Sets the OAuth2 client ID (required).</summary>
        public Builder ClientId(string clientId)
        {
            _options.ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            return this;
        }

        /// <summary>Sets the OAuth2 client secret.</summary>
        public Builder ClientSecret(string clientSecret)
        {
            _options.ClientSecret = clientSecret;
            return this;
        }

        /// <summary>Sets the PEM-encoded EC private key for private_key_jwt (ES256) authentication.</summary>
        public Builder PrivateKey(string privateKey)
        {
            _options.PrivateKey = privateKey;
            return this;
        }

        /// <summary>Sets the key ID for private_key_jwt authentication.</summary>
        public Builder Kid(string kid)
        {
            _options.Kid = kid;
            return this;
        }

        /// <summary>Sets the API base URL. Defaults to https://api.signdocs.com.br.</summary>
        public Builder BaseUrl(string baseUrl)
        {
            _options.BaseUrl = baseUrl;
            return this;
        }

        /// <summary>Sets the HTTP request timeout. Defaults to 30 seconds.</summary>
        public Builder Timeout(TimeSpan timeout)
        {
            _options.Timeout = timeout;
            return this;
        }

        /// <summary>Sets the maximum number of retry attempts for retryable errors (429, 500, 503). Defaults to 5.</summary>
        public Builder MaxRetries(int maxRetries)
        {
            _options.MaxRetries = maxRetries;
            return this;
        }

        /// <summary>Sets the OAuth2 scopes to request.</summary>
        public Builder Scopes(params string[] scopes)
        {
            _options.Scopes = scopes;
            return this;
        }

        /// <summary>Sets a custom HttpClient to use for API requests.</summary>
        public Builder HttpClient(HttpClient httpClient)
        {
            _options.HttpClient = httpClient;
            return this;
        }

        /// <summary>Sets an ILogger for request/response logging.</summary>
        public Builder Logger(ILogger logger)
        {
            _options.Logger = logger;
            return this;
        }

        /// <summary>
        /// Sets a custom <see cref="ITokenCache"/>. Defaults to an in-process
        /// <see cref="InMemoryTokenCache"/>.
        /// </summary>
        public Builder TokenCache(ITokenCache tokenCache)
        {
            _options.TokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
            return this;
        }

        /// <summary>
        /// Registers an observer invoked once per API response with rate-limit,
        /// deprecation, and request-ID metadata. Callback exceptions are swallowed.
        /// </summary>
        public Builder OnResponse(Action<ResponseMetadata> onResponse)
        {
            _options.OnResponse = onResponse ?? throw new ArgumentNullException(nameof(onResponse));
            return this;
        }

        /// <summary>
        /// Builds and returns the configured <see cref="SignDocsBrasilClient"/>.
        /// </summary>
        /// <exception cref="ArgumentException">If the configuration is invalid.</exception>
        public SignDocsBrasilClient Build()
        {
            return new SignDocsBrasilClient(_options);
        }
    }
}
