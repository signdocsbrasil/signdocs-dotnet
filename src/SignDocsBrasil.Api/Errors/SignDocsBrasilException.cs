namespace SignDocsBrasil.Api.Errors;

public class SignDocsBrasilException : Exception
{
    public SignDocsBrasilException(string message) : base(message) { }

    public SignDocsBrasilException(string message, Exception innerException)
        : base(message, innerException) { }
}
