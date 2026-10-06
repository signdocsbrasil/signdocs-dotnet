namespace SignDocsBrasil.Api.Errors;

public class ApiException : SignDocsBrasilException
{
    public ProblemDetail ProblemDetail { get; }

    public int Status => ProblemDetail.Status;
    public string? Type => ProblemDetail.Type;
    public string? Title => ProblemDetail.Title;
    public string? Detail => ProblemDetail.Detail;
    public string? Instance => ProblemDetail.Instance;
    /// <summary>Stable machine-readable case, when the API gives one (e.g. <c>TIMESTAMP_UNAVAILABLE</c>).</summary>
    public string? Code => ProblemDetail.Code;

    public ApiException(ProblemDetail problemDetail)
        : base(problemDetail.Detail ?? problemDetail.Title ?? $"HTTP {problemDetail.Status}")
    {
        ProblemDetail = problemDetail;
    }
}
