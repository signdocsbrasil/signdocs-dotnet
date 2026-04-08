namespace SignDocsBrasil.Api.Errors;

public class ApiException : SignDocsBrasilException
{
    public ProblemDetail ProblemDetail { get; }

    public int Status => ProblemDetail.Status;
    public string? Type => ProblemDetail.Type;
    public string? Title => ProblemDetail.Title;
    public string? Detail => ProblemDetail.Detail;
    public string? Instance => ProblemDetail.Instance;

    public ApiException(ProblemDetail problemDetail)
        : base(problemDetail.Detail ?? problemDetail.Title ?? $"HTTP {problemDetail.Status}")
    {
        ProblemDetail = problemDetail;
    }
}
