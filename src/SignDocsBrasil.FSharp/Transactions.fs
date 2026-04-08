namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Transactions =
    /// Creates a new transaction.
    let create (request: CreateTransactionRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Transactions.CreateAsync(request))

    /// Lists transactions with optional parameters.
    let list (parameters: TransactionListParams option) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Transactions.ListAsync(parameters |> Option.defaultValue null))

    /// Gets a transaction by ID.
    let get (transactionId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Transactions.GetAsync(transactionId))

    /// Cancels a transaction by ID.
    let cancel (transactionId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Transactions.CancelAsync(transactionId))

    /// Finalizes a transaction by ID.
    let finalize (transactionId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Transactions.FinalizeAsync(transactionId))

    /// Returns the IAsyncEnumerable directly (consumers use taskSeq or for..in).
    let listAll (parameters: TransactionListParams option) (client: SignDocsBrasilClient) =
        client.Transactions.ListAutoPaginateAsync(parameters |> Option.defaultValue null)
