namespace SignDocsBrasil.Api.Errors;

public class UnprocessableEntityException : ApiException
{
    public UnprocessableEntityException(ProblemDetail problemDetail) : base(problemDetail) { }
}
