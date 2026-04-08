namespace SignDocsBrasil.Api.Errors;

public class RateLimitException : ApiException
{
    public int? RetryAfterSeconds { get; }

    public RateLimitException(ProblemDetail problemDetail, int? retryAfterSeconds = null)
        : base(problemDetail)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
