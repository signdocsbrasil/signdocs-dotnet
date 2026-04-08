namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Health =
    /// Performs a health check against the API.
    let check (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Health.CheckAsync())

    /// Retrieves health check history.
    let history (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Health.HistoryAsync())
