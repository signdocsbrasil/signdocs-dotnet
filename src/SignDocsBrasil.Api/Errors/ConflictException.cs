namespace SignDocsBrasil.Api.Errors;

public class ConflictException : ApiException
{
    public ConflictException(ProblemDetail problemDetail) : base(problemDetail) { }
}
