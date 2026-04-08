namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api.Errors

/// Discriminated union representing all possible SDK errors.
/// Enables exhaustive pattern matching instead of try/catch.
type SignDocsError =
    | BadRequest of ProblemDetail
    | Unauthorized of ProblemDetail
    | Forbidden of ProblemDetail
    | NotFound of ProblemDetail
    | Conflict of ProblemDetail
    | UnprocessableEntity of ProblemDetail
    | RateLimit of ProblemDetail * retryAfterSeconds: int option
    | InternalServer of ProblemDetail
    | ServiceUnavailable of ProblemDetail
    | ApiError of ProblemDetail
    | AuthenticationFailed of message: string
    | NetworkError of message: string
    | Timeout of message: string

module SignDocsError =
    /// Maps a C# exception to the F# SignDocsError discriminated union.
    let ofException (ex: exn) : SignDocsError =
        match ex with
        | :? BadRequestException as e -> BadRequest e.ProblemDetail
        | :? UnauthorizedException as e -> Unauthorized e.ProblemDetail
        | :? ForbiddenException as e -> Forbidden e.ProblemDetail
        | :? NotFoundException as e -> NotFound e.ProblemDetail
        | :? ConflictException as e -> Conflict e.ProblemDetail
        | :? UnprocessableEntityException as e -> UnprocessableEntity e.ProblemDetail
        | :? RateLimitException as e -> RateLimit(e.ProblemDetail, e.RetryAfterSeconds |> Option.ofNullable)
        | :? InternalServerException as e -> InternalServer e.ProblemDetail
        | :? ServiceUnavailableException as e -> ServiceUnavailable e.ProblemDetail
        | :? ApiException as e -> ApiError e.ProblemDetail
        | :? AuthenticationException as e -> AuthenticationFailed e.Message
        | :? ConnectionException as e -> NetworkError e.Message
        | :? SignDocsTimeoutException as e -> Timeout e.Message
        | _ -> NetworkError ex.Message
