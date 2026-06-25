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

    /// Inspects an arbitrary PDF for embedded signatures. Authenticated
    /// (requires the verification:write scope) and production-credentials-only.
    let verifyDocument (request: VerifyDocumentRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Verification.VerifyDocumentAsync(request))
