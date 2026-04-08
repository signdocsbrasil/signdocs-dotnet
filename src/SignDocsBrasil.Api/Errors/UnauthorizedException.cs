namespace SignDocsBrasil.Api.Errors;

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(ProblemDetail problemDetail) : base(problemDetail) { }
}
