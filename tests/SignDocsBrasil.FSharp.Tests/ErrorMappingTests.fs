module SignDocsBrasil.FSharp.Tests.ErrorMappingTests

open Xunit
open SignDocsBrasil.Api.Errors
open SignDocsBrasil.FSharp

let private problemDetail =
    ProblemDetail("https://example.com/error", "Test Error", 400, "Something went wrong", "/test")

[<Fact>]
let ``BadRequestException maps to BadRequest`` () =
    let ex = BadRequestException(problemDetail)
    let result = SignDocsError.ofException ex

    match result with
    | BadRequest pd -> Assert.Equal(400, pd.Status)
    | _ -> Assert.Fail("Expected BadRequest")

[<Fact>]
let ``UnauthorizedException maps to Unauthorized`` () =
    let pd = ProblemDetail("https://example.com/error", "Unauthorized", 401, "Invalid token", "/test")
    let ex = UnauthorizedException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | Unauthorized pd -> Assert.Equal(401, pd.Status)
    | _ -> Assert.Fail("Expected Unauthorized")

[<Fact>]
let ``ForbiddenException maps to Forbidden`` () =
    let pd = ProblemDetail("https://example.com/error", "Forbidden", 403, "Access denied", "/test")
    let ex = ForbiddenException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | Forbidden pd -> Assert.Equal(403, pd.Status)
    | _ -> Assert.Fail("Expected Forbidden")

[<Fact>]
let ``NotFoundException maps to NotFound`` () =
    let pd = ProblemDetail("https://example.com/error", "Not Found", 404, "Resource not found", "/test")
    let ex = NotFoundException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | NotFound pd -> Assert.Equal(404, pd.Status)
    | _ -> Assert.Fail("Expected NotFound")

[<Fact>]
let ``ConflictException maps to Conflict`` () =
    let pd = ProblemDetail("https://example.com/error", "Conflict", 409, "Resource conflict", "/test")
    let ex = ConflictException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | Conflict pd -> Assert.Equal(409, pd.Status)
    | _ -> Assert.Fail("Expected Conflict")

[<Fact>]
let ``UnprocessableEntityException maps to UnprocessableEntity`` () =
    let pd = ProblemDetail("https://example.com/error", "Unprocessable", 422, "Validation failed", "/test")
    let ex = UnprocessableEntityException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | UnprocessableEntity pd -> Assert.Equal(422, pd.Status)
    | _ -> Assert.Fail("Expected UnprocessableEntity")

[<Fact>]
let ``RateLimitException maps to RateLimit with RetryAfterSeconds`` () =
    let pd = ProblemDetail("https://example.com/error", "Rate Limited", 429, "Too many requests", "/test")
    let ex = RateLimitException(pd, System.Nullable<int>(60))
    let result = SignDocsError.ofException ex

    match result with
    | RateLimit(pd, retryAfter) ->
        Assert.Equal(429, pd.Status)
        Assert.Equal(Some 60, retryAfter)
    | _ -> Assert.Fail("Expected RateLimit")

[<Fact>]
let ``RateLimitException maps to RateLimit without RetryAfterSeconds`` () =
    let pd = ProblemDetail("https://example.com/error", "Rate Limited", 429, "Too many requests", "/test")
    let ex = RateLimitException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | RateLimit(pd, retryAfter) ->
        Assert.Equal(429, pd.Status)
        Assert.Equal(None, retryAfter)
    | _ -> Assert.Fail("Expected RateLimit")

[<Fact>]
let ``InternalServerException maps to InternalServer`` () =
    let pd = ProblemDetail("https://example.com/error", "Internal Error", 500, "Server error", "/test")
    let ex = InternalServerException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | InternalServer pd -> Assert.Equal(500, pd.Status)
    | _ -> Assert.Fail("Expected InternalServer")

[<Fact>]
let ``ServiceUnavailableException maps to ServiceUnavailable`` () =
    let pd = ProblemDetail("https://example.com/error", "Unavailable", 503, "Service down", "/test")
    let ex = ServiceUnavailableException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | ServiceUnavailable pd -> Assert.Equal(503, pd.Status)
    | _ -> Assert.Fail("Expected ServiceUnavailable")

[<Fact>]
let ``ApiException maps to ApiError`` () =
    let pd = ProblemDetail("https://example.com/error", "API Error", 418, "I'm a teapot", "/test")
    let ex = ApiException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | ApiError pd -> Assert.Equal(418, pd.Status)
    | _ -> Assert.Fail("Expected ApiError")

[<Fact>]
let ``AuthenticationException maps to AuthenticationFailed`` () =
    let ex = AuthenticationException("Token refresh failed")
    let result = SignDocsError.ofException ex

    match result with
    | AuthenticationFailed msg -> Assert.Equal("Token refresh failed", msg)
    | _ -> Assert.Fail("Expected AuthenticationFailed")

[<Fact>]
let ``ConnectionException maps to NetworkError`` () =
    let ex = ConnectionException("Connection refused")
    let result = SignDocsError.ofException ex

    match result with
    | NetworkError msg -> Assert.Equal("Connection refused", msg)
    | _ -> Assert.Fail("Expected NetworkError")

[<Fact>]
let ``SignDocsTimeoutException maps to Timeout`` () =
    let ex = SignDocsTimeoutException("Request timed out after 30s")
    let result = SignDocsError.ofException ex

    match result with
    | Timeout msg -> Assert.Equal("Request timed out after 30s", msg)
    | _ -> Assert.Fail("Expected Timeout")

[<Fact>]
let ``Unknown exception maps to NetworkError`` () =
    let ex = System.InvalidOperationException("Something unexpected")
    let result = SignDocsError.ofException ex

    match result with
    | NetworkError msg -> Assert.Equal("Something unexpected", msg)
    | _ -> Assert.Fail("Expected NetworkError for unknown exception")

[<Fact>]
let ``ProblemDetail fields are preserved through mapping`` () =
    let pd = ProblemDetail("https://api.signdocs.com.br/errors/400", "Bad Request", 400, "Invalid field 'name'", "/v1/transactions")
    let ex = BadRequestException(pd)
    let result = SignDocsError.ofException ex

    match result with
    | BadRequest mapped ->
        Assert.Equal("https://api.signdocs.com.br/errors/400", mapped.Type)
        Assert.Equal("Bad Request", mapped.Title)
        Assert.Equal(400, mapped.Status)
        Assert.Equal("Invalid field 'name'", mapped.Detail)
        Assert.Equal("/v1/transactions", mapped.Instance)
    | _ -> Assert.Fail("Expected BadRequest")
