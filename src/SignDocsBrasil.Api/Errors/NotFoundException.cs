namespace SignDocsBrasil.Api.Errors;

public class NotFoundException : ApiException
{
    public NotFoundException(ProblemDetail problemDetail) : base(problemDetail) { }
}
