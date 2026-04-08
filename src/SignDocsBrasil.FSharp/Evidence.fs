namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Evidence =
    /// Gets the evidence for a transaction.
    let get (transactionId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Evidence.GetAsync(transactionId))
