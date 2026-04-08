namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Documents =
    /// Uploads a document to a transaction.
    let upload (transactionId: string) (request: UploadDocumentRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Documents.UploadAsync(transactionId, request))

    /// Gets a presigned URL for document upload.
    let presign (transactionId: string) (request: PresignRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Documents.PresignAsync(transactionId, request))

    /// Confirms a document upload.
    let confirm (transactionId: string) (request: ConfirmDocumentRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Documents.ConfirmAsync(transactionId, request))

    /// Downloads the document for a transaction.
    let download (transactionId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Documents.DownloadAsync(transactionId))
