namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Steps =
    /// Lists steps for a transaction.
    let list (transactionId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Steps.ListAsync(transactionId))

    /// Starts a step within a transaction.
    let start
        (transactionId: string)
        (stepId: string)
        (request: StartStepRequest option)
        (client: SignDocsBrasilClient)
        =
        tryCallAsync (fun () -> client.Steps.StartAsync(transactionId, stepId, request |> Option.defaultValue null))

    /// Completes a step within a transaction.
    let complete
        (transactionId: string)
        (stepId: string)
        (body: obj option)
        (client: SignDocsBrasilClient)
        =
        tryCallAsync (fun () -> client.Steps.CompleteAsync(transactionId, stepId, body |> Option.defaultValue null))
