namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Signing =
    /// Prepares a signing operation for a transaction.
    let prepare (transactionId: string) (request: PrepareSigningRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Signing.PrepareAsync(transactionId, request))

    /// Completes a signing operation for a transaction.
    let complete (transactionId: string) (request: CompleteSigningRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Signing.CompleteAsync(transactionId, request))
