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

    /// <summary>Enrols up to 25 users in one request.</summary>
    /// <remarks>
    /// The documented cap is 25 rows, but the binding limit is the request body
    /// — roughly 6MB, and base64 inflates each photo by a third. Keep photos
    /// under ~175KB (640x640 is ample) to use all 25 slots.
    ///
    /// Set DryRun on the request to inspect the photos without storing anything.
    /// </remarks>
    public async Task<EnrollUsersBatchResponse?> EnrollBatchAsync(
        EnrollUsersBatchRequest request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<EnrollUsersBatchResponse>(
            HttpMethod.Post,
            "/v1/users/enrollments",
            body: request,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }
}
