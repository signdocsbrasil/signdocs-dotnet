namespace SignDocsBrasil.Api.Errors;

public class SignDocsTimeoutException : SignDocsBrasilException
{
    public SignDocsTimeoutException(string message) : base(message) { }

    public SignDocsTimeoutException(string message, Exception innerException)
        : base(message, innerException) { }
}
