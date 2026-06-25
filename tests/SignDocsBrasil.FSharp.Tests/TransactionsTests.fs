module SignDocsBrasil.FSharp.Tests.TransactionsTests

open Xunit
open SignDocsBrasil.FSharp

/// Verifies that module functions have the correct return type shape.
/// This is a compile-time type check: if the types change, this test won't compile.
[<Fact>]
let ``Transactions module functions return correct types`` () =
    // This test validates at compile time that the function signatures are correct.
    // We check the type of each function by assigning to typed bindings.
    let _create: SignDocsBrasil.Api.Models.CreateTransactionRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.Transaction, SignDocsError>> =
        Transactions.create

    let _get: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.Transaction, SignDocsError>> =
        Transactions.get

    let _cancel: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.CancelTransactionResponse, SignDocsError>> =
        Transactions.cancel

    let _finalize: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.FinalizeResponse, SignDocsError>> =
        Transactions.finalize

    let _list: SignDocsBrasil.Api.Models.TransactionListParams option -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.TransactionListResponse, SignDocsError>> =
        Transactions.list

    // If this compiles, the types are correct.
    Assert.True(true)

/// Verifies that listAll returns an IAsyncEnumerable.
[<Fact>]
let ``Transactions listAll returns IAsyncEnumerable`` () =
    let _listAll: SignDocsBrasil.Api.Models.TransactionListParams option -> SignDocsBrasil.Api.SignDocsBrasilClient -> System.Collections.Generic.IAsyncEnumerable<SignDocsBrasil.Api.Models.Transaction> =
        Transactions.listAll

    Assert.True(true)

/// Verifies that Documents module functions have the correct return type shape.
[<Fact>]
let ``Documents module functions return correct types`` () =
    let _upload: string -> SignDocsBrasil.Api.Models.UploadDocumentRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.DocumentUploadResponse, SignDocsError>> =
        Documents.upload

    let _presign: string -> SignDocsBrasil.Api.Models.PresignRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.PresignResponse, SignDocsError>> =
        Documents.presign

    let _confirm: string -> SignDocsBrasil.Api.Models.ConfirmDocumentRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.ConfirmDocumentResponse, SignDocsError>> =
        Documents.confirm

    let _download: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.DownloadResponse, SignDocsError>> =
        Documents.download

    Assert.True(true)

/// Verifies that Steps module functions have the correct return type shape.
[<Fact>]
let ``Steps module functions return correct types`` () =
    let _list: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.StepListResponse, SignDocsError>> =
        Steps.list

    let _start: string -> string -> SignDocsBrasil.Api.Models.StartStepRequest option -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.StartStepResponse, SignDocsError>> =
        Steps.start

    let _complete: string -> string -> obj option -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.StepCompleteResponse, SignDocsError>> =
        Steps.complete

    Assert.True(true)

/// Verifies that Signing module functions have the correct return type shape.
[<Fact>]
let ``Signing module functions return correct types`` () =
    let _prepare: string -> SignDocsBrasil.Api.Models.PrepareSigningRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.PrepareSigningResponse, SignDocsError>> =
        Signing.prepare

    let _complete: string -> SignDocsBrasil.Api.Models.CompleteSigningRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.CompleteSigningResponse, SignDocsError>> =
        Signing.complete

    Assert.True(true)

/// Verifies that Evidence module functions have the correct return type shape.
[<Fact>]
let ``Evidence module functions return correct types`` () =
    let _get: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.Evidence, SignDocsError>> =
        Evidence.get

    Assert.True(true)

/// Verifies that Verification module functions have the correct return type shape.
[<Fact>]
let ``Verification module functions return correct types`` () =
    let _verify: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.VerificationResponse, SignDocsError>> =
        Verification.verify

    let _downloads: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.VerificationDownloadsResponse, SignDocsError>> =
        Verification.downloads

    let _verifyDocument: SignDocsBrasil.Api.Models.VerifyDocumentRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.VerifyDocumentResponse, SignDocsError>> =
        Verification.verifyDocument

    Assert.True(true)

/// Verifies that Users module functions have the correct return type shape.
[<Fact>]
let ``Users module functions return correct types`` () =
    let _enroll: string -> SignDocsBrasil.Api.Models.EnrollUserRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.EnrollUserResponse, SignDocsError>> =
        Users.enroll

    Assert.True(true)

/// Verifies that Health module functions have the correct return type shape.
[<Fact>]
let ``Health module functions return correct types`` () =
    let _check: SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.HealthCheckResponse, SignDocsError>> =
        Health.check

    let _history: SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.HealthHistoryResponse, SignDocsError>> =
        Health.history

    Assert.True(true)

/// Verifies that DocumentGroups module functions have the correct return type shape.
[<Fact>]
let ``DocumentGroups module functions return correct types`` () =
    let _combinedStamp: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.CombinedStampResponse, SignDocsError>> =
        DocumentGroups.combinedStamp

    Assert.True(true)
