namespace SignDocsBrasil.Api.Errors;

public record ProblemDetail(string? Type, string? Title, int Status, string? Detail, string? Instance)
{
    public static ProblemDetail Fallback(int status, string? body) => new(
        $"https://api.signdocs.com.br/errors/{status}", $"HTTP {status}", status, body, null);
}
