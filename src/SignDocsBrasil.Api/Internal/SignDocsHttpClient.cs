using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SignDocsBrasil.Api.Errors;

namespace SignDocsBrasil.Api.Internal;

/// <summary>
/// HTTP client wrapper for the SignDocsBrasil API.
/// Handles authentication, JSON serialization, retry logic, and error parsing.
/// Wraps <see cref="System.Net.Http.HttpClient"/>.
/// </summary>
internal sealed class SignDocsHttpClient : IDisposable
{
    internal const string SdkVersion = "1.0.0";
    internal const string UserAgent = "signdocs-brasil-dotnet/" + SdkVersion;

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly string _baseUrl;
    private readonly TimeSpan _timeout;
    private readonly AuthHandler _auth;
    private readonly int _maxRetries;
    private readonly ILogger _logger;

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Creates a new <see cref="SignDocsHttpClient"/>.
    /// </summary>
    /// <param name="httpClient">
    /// User-provided <see cref="HttpClient"/>, or <c>null</c> to create one internally
    /// with <see cref="Timeout.InfiniteTimeSpan"/> (per-request timeouts are used instead).
    /// </param>
    /// <param name="baseUrl">The API base URL (e.g., "https://api.signdocs.com.br").</param>
    /// <param name="timeout">Default per-request timeout.</param>
    /// <param name="auth">The authentication handler for acquiring Bearer tokens.</param>
    /// <param name="maxRetries">Maximum number of retry attempts for retryable errors.</param>
    /// <param name="logger">Logger instance, or <c>null</c> for no logging.</param>
    internal SignDocsHttpClient(
        HttpClient? httpClient,
        string baseUrl,
        TimeSpan timeout,
        AuthHandler auth,
        int maxRetries,
        ILogger? logger)
    {
        if (httpClient is not null)
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _httpClient = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            _ownsHttpClient = true;
        }

        _baseUrl = baseUrl.TrimEnd('/');
        _timeout = timeout;
        _auth = auth;
        _maxRetries = maxRetries;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Executes an authenticated HTTP request with retry logic.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <param name="method">HTTP method.</param>
    /// <param name="path">API path (e.g., "/v1/transactions").</param>
    /// <param name="body">Request body object, or <c>null</c> for no body.</param>
    /// <param name="query">Optional query parameters.</param>
    /// <param name="headers">Optional additional headers.</param>
    /// <param name="noAuth">If <c>true</c>, skip adding the Authorization header.</param>
    /// <param name="timeout">Optional per-request timeout override.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The deserialized response, or <c>default(T)</c> for 204 No Content.</returns>
    internal async Task<T?> RequestAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        Dictionary<string, string>? query = null,
        Dictionary<string, string>? headers = null,
        bool noAuth = false,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        long startTime = Environment.TickCount64;

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            RetryPolicy.CheckTimeout(startTime);

            (HttpResponseMessage response, long durationMs) = await ExecuteRequestAsync(
                method, path, body, query, headers, noAuth, timeout, ct).ConfigureAwait(false);

            using (response)
            {
                int statusCode = (int)response.StatusCode;

                // Log the request/response
                if (statusCode >= 400)
                {
                    _logger.LogWarning("{Method} {Path} -> {Status} ({Duration}ms)",
                        method, path, statusCode, durationMs);
                }
                else
                {
                    _logger.LogInformation("{Method} {Path} -> {Status} ({Duration}ms)",
                        method, path, statusCode, durationMs);
                }

                // If retryable and not on last attempt, retry with backoff
                if (RetryPolicy.IsRetryable(statusCode) && attempt < _maxRetries)
                {
                    long delay = RetryPolicy.CalculateDelay(attempt, response);
                    await RetryPolicy.DelayAsync(delay, ct).ConfigureAwait(false);
                    continue;
                }

                // Parse the response
                return await ParseResponseAsync<T>(response, ct).ConfigureAwait(false);
            }
        }

        throw new SignDocsTimeoutException("Max retries exceeded");
    }

    /// <summary>
    /// Executes a request with an automatic idempotency key via the X-Idempotency-Key header.
    /// </summary>
    internal async Task<T?> RequestWithIdempotencyAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        string? idempotencyKey = null,
        Dictionary<string, string>? query = null,
        Dictionary<string, string>? headers = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        string key = idempotencyKey ?? Guid.NewGuid().ToString();
        var mergedHeaders = headers is not null
            ? new Dictionary<string, string>(headers)
            : new Dictionary<string, string>();
        mergedHeaders["X-Idempotency-Key"] = key;

        return await RequestAsync<T>(method, path, body, query, mergedHeaders, noAuth: false, timeout, ct)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an unauthenticated request (skips the Bearer token).
    /// </summary>
    internal async Task<T?> RequestNoAuthAsync<T>(
        HttpMethod method,
        string path,
        Dictionary<string, string>? query = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await RequestAsync<T>(method, path, body: null, query, headers: null, noAuth: true, timeout, ct)
            .ConfigureAwait(false);
    }

    private async Task<(HttpResponseMessage Response, long DurationMs)> ExecuteRequestAsync(
        HttpMethod method,
        string path,
        object? body,
        Dictionary<string, string>? query,
        Dictionary<string, string>? extraHeaders,
        bool noAuth,
        TimeSpan? requestTimeout,
        CancellationToken ct)
    {
        TimeSpan effectiveTimeout = requestTimeout ?? _timeout;

        // Per-request timeout via linked CancellationTokenSource
        using var timeoutCts = new CancellationTokenSource(effectiveTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
        CancellationToken linkedToken = linkedCts.Token;

        try
        {
            string url = BuildUrl(path, query);
            using var request = new HttpRequestMessage(method, url);

            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

            // Add authentication
            if (!noAuth)
            {
                string token = await _auth.GetAccessTokenAsync(linkedToken).ConfigureAwait(false);
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
            }

            // Add extra headers
            if (extraHeaders is not null)
            {
                foreach (KeyValuePair<string, string> header in extraHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Set body
            if (body is not null)
            {
                string jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }
            else if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                // POST/PUT with null body sends "{}"
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            }

            long sw = Stopwatch.GetTimestamp();
            HttpResponseMessage response = await _httpClient
                .SendAsync(request, linkedToken)
                .ConfigureAwait(false);
            long durationMs = (long)Stopwatch.GetElapsedTime(sw).TotalMilliseconds;

            return (response, durationMs);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            throw new SignDocsTimeoutException(
                $"Request to {path} timed out after {effectiveTimeout.TotalMilliseconds}ms");
        }
        catch (OperationCanceledException)
        {
            // Caller's cancellation token was triggered — rethrow as-is
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new ConnectionException(
                $"Failed to connect to {_baseUrl}{path}: {ex.Message}", ex);
        }
    }

    private static async Task<T?> ParseResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        int statusCode = (int)response.StatusCode;

        // Handle 204 No Content
        if (statusCode == 204)
        {
            return default;
        }

        string responseBody = await response.Content
            .ReadAsStringAsync(ct)
            .ConfigureAwait(false);

        string? contentType = response.Content.Headers.ContentType?.MediaType;

        // Handle error responses
        if (statusCode >= 400)
        {
            ThrowApiError(statusCode, responseBody, contentType, response);
        }

        // Handle empty body
        if (string.IsNullOrEmpty(responseBody))
        {
            return default;
        }

        // Deserialize response
        return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
    }

    private static void ThrowApiError(
        int statusCode,
        string body,
        string? contentType,
        HttpResponseMessage response)
    {
        ProblemDetail problemDetail;

        bool isJson = contentType is not null
            && (contentType.Contains("application/json") || contentType.Contains("application/problem+json"));

        if (isJson && !string.IsNullOrEmpty(body))
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("type", out _))
                {
                    problemDetail = JsonSerializer.Deserialize<ProblemDetail>(body, JsonOptions)
                        ?? ProblemDetail.Fallback(statusCode, body);
                }
                else
                {
                    problemDetail = ProblemDetail.Fallback(statusCode, body);
                }
            }
            catch
            {
                problemDetail = ProblemDetail.Fallback(statusCode, body);
            }
        }
        else
        {
            problemDetail = ProblemDetail.Fallback(statusCode, body);
        }

        int? retryAfterSeconds = null;
        if (statusCode == 429)
        {
            if (response.Headers.RetryAfter?.Delta is TimeSpan delta)
            {
                retryAfterSeconds = (int)delta.TotalSeconds;
            }
            else if (response.Headers.TryGetValues("Retry-After", out IEnumerable<string>? values))
            {
                string? raw = values.FirstOrDefault();
                if (raw is not null && int.TryParse(raw, out int parsed))
                {
                    retryAfterSeconds = parsed;
                }
            }
        }

        throw statusCode switch
        {
            400 => new BadRequestException(problemDetail),
            401 => new UnauthorizedException(problemDetail),
            403 => new ForbiddenException(problemDetail),
            404 => new NotFoundException(problemDetail),
            409 => new ConflictException(problemDetail),
            422 => new UnprocessableEntityException(problemDetail),
            429 => new RateLimitException(problemDetail, retryAfterSeconds),
            500 => new InternalServerException(problemDetail),
            503 => new ServiceUnavailableException(problemDetail),
            _   => new ApiException(problemDetail)
        };
    }

    /// <summary>
    /// Builds a full URL from the base URL, path, and optional query parameters.
    /// Query parameter keys and values are URL-encoded.
    /// </summary>
    internal string BuildUrl(string path, Dictionary<string, string>? query)
    {
        var sb = new StringBuilder(_baseUrl);
        sb.Append(path);

        if (query is { Count: > 0 })
        {
            sb.Append('?');
            bool first = true;
            foreach (KeyValuePair<string, string> entry in query)
            {
                if (entry.Value is null)
                {
                    continue;
                }

                if (!first)
                {
                    sb.Append('&');
                }

                sb.Append(Uri.EscapeDataString(entry.Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(entry.Value));
                first = false;
            }
        }

        return sb.ToString();
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
