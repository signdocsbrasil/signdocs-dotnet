namespace SignDocsBrasil.Api.Errors;

public class ServiceUnavailableException : ApiException
{
    public ServiceUnavailableException(ProblemDetail problemDetail) : base(problemDetail) { }
}
