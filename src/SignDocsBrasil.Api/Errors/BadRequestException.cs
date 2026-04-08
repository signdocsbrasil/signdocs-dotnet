namespace SignDocsBrasil.Api.Errors;

public class BadRequestException : ApiException
{
    public BadRequestException(ProblemDetail problemDetail) : base(problemDetail) { }
}
