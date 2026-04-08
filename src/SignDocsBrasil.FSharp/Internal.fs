namespace SignDocsBrasil.FSharp

open System.Threading.Tasks

[<AutoOpen>]
module internal Internal =
    /// Wraps a C# async call in F# Async<Result<'a, SignDocsError>>.
    /// Catches all SDK exceptions and maps them to the SignDocsError DU.
    let tryCallAsync (f: unit -> Task<'a>) : Async<Result<'a, SignDocsError>> =
        async {
            try
                let! result = f () |> Async.AwaitTask
                return Ok result
            with ex ->
                return Error(SignDocsError.ofException ex)
        }

    /// Wraps a C# async Task (no return value) call.
    let tryCallTaskAsync (f: unit -> Task) : Async<Result<unit, SignDocsError>> =
        async {
            try
                do! f () |> Async.AwaitTask
                return Ok()
            with ex ->
                return Error(SignDocsError.ofException ex)
        }
