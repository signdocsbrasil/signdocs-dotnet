namespace SignDocsBrasil.Api.TokenCache;

/// <summary>
/// Immutable value object representing a cached OAuth2 access token
/// along with its absolute expiry timestamp.
/// </summary>
/// <param name="AccessToken">The raw Bearer access token.</param>
/// <param name="ExpiresAt">The absolute UTC timestamp after which the token is considered expired.</param>
public sealed record CachedToken(string AccessToken, DateTimeOffset ExpiresAt)
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="now"/> is at or past
    /// <see cref="ExpiresAt"/> minus <paramref name="skew"/>.
    /// </summary>
    /// <param name="now">The current time to compare against.</param>
    /// <param name="skew">
    /// Safety margin subtracted from <see cref="ExpiresAt"/> to avoid using a token
    /// that is about to expire mid-request. A typical value is 30 seconds.
    /// </param>
    public bool IsExpired(DateTimeOffset now, TimeSpan skew) => now >= ExpiresAt - skew;
}
