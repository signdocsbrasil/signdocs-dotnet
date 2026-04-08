namespace SignDocsBrasil.Api.Errors;

public class AuthenticationException : SignDocsBrasilException
{
    public AuthenticationException(string message) : base(message) { }

    public AuthenticationException(string message, Exception innerException)
        : base(message, innerException) { }
}
