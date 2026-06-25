using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class SigningSessionsResource
{
    private readonly SignDocsHttpClient _client;

    internal SigningSessionsResource(SignDocsHttpClient client) => _client = client;

    /// <summary>
    /// Creates a new signing session.
    /// An X-Idempotency-Key header is automatically set. Pass an explicit key
    /// to enable safe retries with the same idempotency guarantee.
    /// </summary>
    public async Task<SigningSession?> CreateAsync(
        CreateSigningSessionRequest request,
        string? idempotencyKey = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestWithIdempotencyAsync<SigningSession>(
            HttpMethod.Post,
            "/v1/signing-sessions",
            body: request,
            idempotencyKey: idempotencyKey,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the status of a signing session, including per-signer progress.
    /// </summary>
    public async Task<SigningSessionStatus?> GetStatusAsync(
        string sessionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<SigningSessionStatus>(
            HttpMethod.Get,
            $"/v1/signing-sessions/{sessionId}/status",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Cancels a signing session.
    /// </summary>
    public async Task<CancelSigningSessionResponse?> CancelAsync(
        string sessionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<CancelSigningSessionResponse>(
            HttpMethod.Post,
            $"/v1/signing-sessions/{sessionId}/cancel",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the full bootstrap data for a signing session.
    /// Used by the embedded signing widget to initialize the UI.
    /// </summary>
    public async Task<SigningSessionBootstrap?> GetAsync(
        string sessionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<SigningSessionBootstrap>(
            HttpMethod.Get,
            $"/v1/signing-sessions/{sessionId}",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Advances a signing session through its steps.
    /// Supports actions: accept, verify_otp, resend_otp, start_liveness,
    /// complete_liveness, prepare_signing, complete_signing.
    /// </summary>
    public async Task<AdvanceSessionResponse?> AdvanceAsync(
        string sessionId,
        AdvanceSessionRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<AdvanceSessionResponse>(
            HttpMethod.Post,
            $"/v1/signing-sessions/{sessionId}/advance",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Resends the OTP challenge for a signing session.
    /// </summary>
    public async Task<AdvanceSessionResponse?> ResendOtpAsync(
        string sessionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<AdvanceSessionResponse>(
            HttpMethod.Post,
            $"/v1/signing-sessions/{sessionId}/resend-otp",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Resends the OTP challenge for a signing session, optionally selecting
    /// the delivery channel (e.g. "sms", "email", "whatsapp").
    /// </summary>
    public async Task<AdvanceSessionResponse?> ResendOtpAsync(
        string sessionId,
        ResendOtpRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<AdvanceSessionResponse>(
            HttpMethod.Post,
            $"/v1/signing-sessions/{sessionId}/resend-otp",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists signing sessions with optional filters and pagination.
    /// </summary>
    public async Task<SigningSessionListResponse?> ListAsync(
        SigningSessionListParams? @params = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        Dictionary<string, string>? query = @params?.ToQueryDictionary();

        return await _client.RequestAsync<SigningSessionListResponse>(
            HttpMethod.Get,
            "/v1/signing-sessions",
            query: query,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Polls a signing session until it reaches a terminal status (COMPLETED, CANCELLED, EXPIRED).
    /// </summary>
    /// <param name="sessionId">The signing session ID.</param>
    /// <param name="pollInterval">Time between polls. Defaults to 3 seconds.</param>
    /// <param name="timeout">Maximum time to wait. Defaults to 5 minutes.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final status when a terminal state is reached.</returns>
    /// <exception cref="TimeoutException">If the timeout is exceeded before a terminal status is reached.</exception>
    public async Task<SigningSessionStatus> WaitForCompletionAsync(
        string sessionId,
        TimeSpan? pollInterval = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        TimeSpan effectivePollInterval = pollInterval ?? TimeSpan.FromSeconds(3);
        TimeSpan effectiveTimeout = timeout ?? TimeSpan.FromMinutes(5);
        string[] terminalStatuses = ["COMPLETED", "CANCELLED", "EXPIRED"];

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(effectiveTimeout);

        while (true)
        {
            cts.Token.ThrowIfCancellationRequested();

            SigningSessionStatus? status = await GetStatusAsync(sessionId, ct: cts.Token)
                .ConfigureAwait(false);

            if (status is not null && Array.Exists(terminalStatuses, s => s == status.Status))
            {
                return status;
            }

            try
            {
                await Task.Delay(effectivePollInterval, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Timed out waiting for signing session {sessionId} to complete. " +
                    $"Current status: {status?.Status}");
            }
        }
    }
}
