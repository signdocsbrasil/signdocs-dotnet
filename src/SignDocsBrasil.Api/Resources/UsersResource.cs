using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class UsersResource
{
    private readonly SignDocsHttpClient _client;

    internal UsersResource(SignDocsHttpClient client) => _client = client;

    public async Task<EnrollUserResponse?> EnrollAsync(
        string userExternalId,
        EnrollUserRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<EnrollUserResponse>(
            HttpMethod.Put,
            $"/v1/users/{userExternalId}/enrollment",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads whether a user is enrolled and, crucially, until when.
    /// </summary>
    /// <remarks>
    /// Use it to sweep your user base and re-enrol before Expired flips.
    /// Nothing warns you on its own beyond the ENROLLMENT.EXPIRING webhook,
    /// and once the grace window closes this throws NotFound rather than
    /// reporting an expired enrolment.
    /// </remarks>
    public async Task<EnrollmentStatusResponse?> GetEnrollmentAsync(
        string userExternalId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<EnrollmentStatusResponse>(
            HttpMethod.Get,
            $"/v1/users/{userExternalId}/enrollment",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Erases a user's biometric enrolment (LGPD art. 18).
    /// </summary>
    /// <remarks>
    /// Destroys every stored version of the reference image, not just the
    /// current one, and removes the record. Irreversible.
    /// </remarks>
    public async Task<DeleteEnrollmentResponse?> DeleteEnrollmentAsync(
        string userExternalId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<DeleteEnrollmentResponse>(
            HttpMethod.Delete,
            $"/v1/users/{userExternalId}/enrollment",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
