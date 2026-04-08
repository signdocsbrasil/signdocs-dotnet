namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Verification =
    /// Verifies an evidence pack by its ID.
    let verify (evidenceId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Verification.VerifyAsync(evidenceId))

    /// Gets download links for a verified evidence pack.
    let downloads (evidenceId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Verification.DownloadsAsync(evidenceId))
