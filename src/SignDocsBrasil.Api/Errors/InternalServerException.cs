namespace SignDocsBrasil.Api.Errors;

public class InternalServerException : ApiException
{
    public InternalServerException(ProblemDetail problemDetail) : base(problemDetail) { }
}
