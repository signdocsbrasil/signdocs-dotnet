namespace SignDocsBrasil.Api.Errors;

public class ForbiddenException : ApiException
{
    public ForbiddenException(ProblemDetail problemDetail) : base(problemDetail) { }
}
