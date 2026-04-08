namespace SignDocsBrasil.Api.Errors;

public class ConnectionException : SignDocsBrasilException
{
    public ConnectionException(string message) : base(message) { }

    public ConnectionException(string message, Exception innerException)
        : base(message, innerException) { }
}
