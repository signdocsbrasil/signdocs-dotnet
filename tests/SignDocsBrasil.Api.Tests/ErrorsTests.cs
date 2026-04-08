using SignDocsBrasil.Api.Errors;

namespace SignDocsBrasil.Api.Tests;

public class ErrorsTests
{
    // --- ProblemDetail ---

    [Fact]
    public void ProblemDetail_Fallback_CreatesCorrectInstance()
    {
        ProblemDetail pd = ProblemDetail.Fallback(400, "some body");

        Assert.Equal("https://api.signdocs.com.br/errors/400", pd.Type);
        Assert.Equal("HTTP 400", pd.Title);
        Assert.Equal(400, pd.Status);
        Assert.Equal("some body", pd.Detail);
        Assert.Null(pd.Instance);
    }

    [Fact]
    public void ProblemDetail_Fallback_500()
    {
        ProblemDetail pd = ProblemDetail.Fallback(500, "server error");

        Assert.Equal("https://api.signdocs.com.br/errors/500", pd.Type);
        Assert.Equal("HTTP 500", pd.Title);
        Assert.Equal(500, pd.Status);
    }

    [Fact]
    public void ProblemDetail_Fallback_NullBody()
    {
        ProblemDetail pd = ProblemDetail.Fallback(503, null);

        Assert.Null(pd.Detail);
    }

    [Fact]
    public void ProblemDetail_Record_Properties()
    {
        var pd = new ProblemDetail(
            "https://example.com/errors/test",
            "Test Error",
            422,
            "Validation failed",
            "/v1/test");

        Assert.Equal("https://example.com/errors/test", pd.Type);
        Assert.Equal("Test Error", pd.Title);
        Assert.Equal(422, pd.Status);
        Assert.Equal("Validation failed", pd.Detail);
        Assert.Equal("/v1/test", pd.Instance);
    }

    // --- SignDocsBrasilException ---

    [Fact]
    public void SignDocsBrasilException_HasMessage()
    {
        var ex = new SignDocsBrasilException("test message");
        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void SignDocsBrasilException_HasInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new SignDocsBrasilException("outer", inner);

        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void SignDocsBrasilException_IsException()
    {
        var ex = new SignDocsBrasilException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }

    // --- ApiException ---

    [Fact]
    public void ApiException_HasCorrectProperties()
    {
        var pd = new ProblemDetail(
            "https://api.signdocs.com.br/errors/bad-request",
            "Bad Request",
            400,
            "Invalid input",
            "/v1/transactions");

        var ex = new ApiException(pd);

        Assert.Same(pd, ex.ProblemDetail);
        Assert.Equal(400, ex.Status);
        Assert.Equal("https://api.signdocs.com.br/errors/bad-request", ex.Type);
        Assert.Equal("Bad Request", ex.Title);
        Assert.Equal("Invalid input", ex.Detail);
        Assert.Equal("/v1/transactions", ex.Instance);
    }

    [Fact]
    public void ApiException_MessageIsDetail()
    {
        var pd = new ProblemDetail("type", "title", 400, "detail message", null);
        var ex = new ApiException(pd);

        Assert.Equal("detail message", ex.Message);
    }

    [Fact]
    public void ApiException_MessageFallsBackToTitle()
    {
        var pd = new ProblemDetail("type", "Title Text", 400, null, null);
        var ex = new ApiException(pd);

        Assert.Equal("Title Text", ex.Message);
    }

    [Fact]
    public void ApiException_MessageFallsBackToStatus()
    {
        var pd = new ProblemDetail("type", null, 400, null, null);
        var ex = new ApiException(pd);

        Assert.Equal("HTTP 400", ex.Message);
    }

    [Fact]
    public void ApiException_InheritsFromSignDocsBrasilException()
    {
        var pd = new ProblemDetail("type", "title", 400, "detail", null);
        var ex = new ApiException(pd);

        Assert.IsAssignableFrom<SignDocsBrasilException>(ex);
    }

    // --- HTTP Status Exceptions ---

    [Fact]
    public void BadRequestException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Bad Request", 400, "detail", null);
        var ex = new BadRequestException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(400, ex.Status);
    }

    [Fact]
    public void UnauthorizedException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Unauthorized", 401, "detail", null);
        var ex = new UnauthorizedException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(401, ex.Status);
    }

    [Fact]
    public void ForbiddenException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Forbidden", 403, "detail", null);
        var ex = new ForbiddenException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(403, ex.Status);
    }

    [Fact]
    public void NotFoundException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Not Found", 404, "detail", null);
        var ex = new NotFoundException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(404, ex.Status);
    }

    [Fact]
    public void ConflictException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Conflict", 409, "detail", null);
        var ex = new ConflictException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(409, ex.Status);
    }

    [Fact]
    public void UnprocessableEntityException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Unprocessable", 422, "detail", null);
        var ex = new UnprocessableEntityException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(422, ex.Status);
    }

    [Fact]
    public void InternalServerException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Internal", 500, "detail", null);
        var ex = new InternalServerException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(500, ex.Status);
    }

    [Fact]
    public void ServiceUnavailableException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Unavailable", 503, "detail", null);
        var ex = new ServiceUnavailableException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(503, ex.Status);
    }

    // --- RateLimitException ---

    [Fact]
    public void RateLimitException_InheritsFromApiException()
    {
        var pd = new ProblemDetail("type", "Rate Limit", 429, "detail", null);
        var ex = new RateLimitException(pd);

        Assert.IsAssignableFrom<ApiException>(ex);
        Assert.Equal(429, ex.Status);
    }

    [Fact]
    public void RateLimitException_HasRetryAfterSeconds()
    {
        var pd = new ProblemDetail("type", "Rate Limit", 429, "detail", null);
        var ex = new RateLimitException(pd, 5);

        Assert.Equal(5, ex.RetryAfterSeconds);
    }

    [Fact]
    public void RateLimitException_RetryAfterSeconds_IsNullByDefault()
    {
        var pd = new ProblemDetail("type", "Rate Limit", 429, "detail", null);
        var ex = new RateLimitException(pd);

        Assert.Null(ex.RetryAfterSeconds);
    }

    // --- Non-API Exceptions ---

    [Fact]
    public void AuthenticationException_InheritsFromSignDocsBrasilException()
    {
        var ex = new AuthenticationException("auth failed");

        Assert.IsAssignableFrom<SignDocsBrasilException>(ex);
        Assert.Equal("auth failed", ex.Message);
    }

    [Fact]
    public void AuthenticationException_HasInnerException()
    {
        var inner = new Exception("inner");
        var ex = new AuthenticationException("outer", inner);

        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void ConnectionException_InheritsFromSignDocsBrasilException()
    {
        var ex = new ConnectionException("connection failed");

        Assert.IsAssignableFrom<SignDocsBrasilException>(ex);
        Assert.Equal("connection failed", ex.Message);
    }

    [Fact]
    public void ConnectionException_HasInnerException()
    {
        var inner = new Exception("inner");
        var ex = new ConnectionException("outer", inner);

        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void SignDocsTimeoutException_InheritsFromSignDocsBrasilException()
    {
        var ex = new SignDocsTimeoutException("timed out");

        Assert.IsAssignableFrom<SignDocsBrasilException>(ex);
        Assert.Equal("timed out", ex.Message);
    }

    [Fact]
    public void SignDocsTimeoutException_HasInnerException()
    {
        var inner = new Exception("inner");
        var ex = new SignDocsTimeoutException("outer", inner);

        Assert.Same(inner, ex.InnerException);
    }

    // --- All exceptions are catchable as SignDocsBrasilException ---

    [Fact]
    public void AllExceptions_AreCatchableAsBaseType()
    {
        var exceptions = new SignDocsBrasilException[]
        {
            new AuthenticationException("auth"),
            new ConnectionException("conn"),
            new SignDocsTimeoutException("timeout"),
            new BadRequestException(new ProblemDetail("t", "t", 400, "d", null)),
            new UnauthorizedException(new ProblemDetail("t", "t", 401, "d", null)),
            new ForbiddenException(new ProblemDetail("t", "t", 403, "d", null)),
            new NotFoundException(new ProblemDetail("t", "t", 404, "d", null)),
            new ConflictException(new ProblemDetail("t", "t", 409, "d", null)),
            new UnprocessableEntityException(new ProblemDetail("t", "t", 422, "d", null)),
            new RateLimitException(new ProblemDetail("t", "t", 429, "d", null)),
            new InternalServerException(new ProblemDetail("t", "t", 500, "d", null)),
            new ServiceUnavailableException(new ProblemDetail("t", "t", 503, "d", null)),
        };

        foreach (var ex in exceptions)
        {
            Assert.IsAssignableFrom<SignDocsBrasilException>(ex);
            Assert.IsAssignableFrom<Exception>(ex);
        }
    }

    [Fact]
    public void AllApiExceptions_AreCatchableAsApiException()
    {
        var pd = new ProblemDetail("t", "t", 400, "d", null);
        ApiException[] exceptions =
        {
            new BadRequestException(pd),
            new UnauthorizedException(new ProblemDetail("t", "t", 401, "d", null)),
            new ForbiddenException(new ProblemDetail("t", "t", 403, "d", null)),
            new NotFoundException(new ProblemDetail("t", "t", 404, "d", null)),
            new ConflictException(new ProblemDetail("t", "t", 409, "d", null)),
            new UnprocessableEntityException(new ProblemDetail("t", "t", 422, "d", null)),
            new RateLimitException(new ProblemDetail("t", "t", 429, "d", null)),
            new InternalServerException(new ProblemDetail("t", "t", 500, "d", null)),
            new ServiceUnavailableException(new ProblemDetail("t", "t", 503, "d", null)),
        };

        foreach (var ex in exceptions)
        {
            Assert.IsAssignableFrom<ApiException>(ex);
            Assert.NotNull(ex.ProblemDetail);
        }
    }
}
