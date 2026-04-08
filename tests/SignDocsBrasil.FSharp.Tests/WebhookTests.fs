module SignDocsBrasil.FSharp.Tests.WebhookTests

open System
open System.Security.Cryptography
open System.Text
open Xunit
open SignDocsBrasil.FSharp

/// Helper: computes the expected HMAC-SHA256 hex signature for a given timestamp and body.
let private computeSignature (secret: string) (timestamp: string) (body: string) =
    let signingInput = timestamp + "." + body
    let keyBytes = Encoding.UTF8.GetBytes(secret)
    let inputBytes = Encoding.UTF8.GetBytes(signingInput)
    let hash = HMACSHA256.HashData(keyBytes, inputBytes)
    BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()

[<Fact>]
let ``verifySignature returns true for valid signature`` () =
    let secret = "whsec_test_secret_key"
    let body = """{"event":"transaction.completed","transactionId":"txn_123"}"""
    let timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
    let signature = computeSignature secret timestamp body

    let result = Webhooks.verifySignature body signature timestamp secret
    Assert.True(result)

[<Fact>]
let ``verifySignature returns false for invalid signature`` () =
    let secret = "whsec_test_secret_key"
    let body = """{"event":"transaction.completed","transactionId":"txn_123"}"""
    let timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()

    let result = Webhooks.verifySignature body "invalidsignature" timestamp secret
    Assert.False(result)

[<Fact>]
let ``verifySignature returns false for tampered body`` () =
    let secret = "whsec_test_secret_key"
    let originalBody = """{"event":"transaction.completed","transactionId":"txn_123"}"""
    let tamperedBody = """{"event":"transaction.completed","transactionId":"txn_456"}"""
    let timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
    let signature = computeSignature secret timestamp originalBody

    let result = Webhooks.verifySignature tamperedBody signature timestamp secret
    Assert.False(result)

[<Fact>]
let ``verifySignature returns false for wrong secret`` () =
    let correctSecret = "whsec_correct_secret"
    let wrongSecret = "whsec_wrong_secret"
    let body = """{"event":"transaction.completed"}"""
    let timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
    let signature = computeSignature correctSecret timestamp body

    let result = Webhooks.verifySignature body signature timestamp wrongSecret
    Assert.False(result)

[<Fact>]
let ``verifySignature returns false for expired timestamp`` () =
    let secret = "whsec_test_secret_key"
    let body = """{"event":"transaction.completed"}"""
    // 10 minutes ago (beyond default 5-minute tolerance)
    let expiredTimestamp = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600L).ToString()
    let signature = computeSignature secret expiredTimestamp body

    let result = Webhooks.verifySignature body signature expiredTimestamp secret
    Assert.False(result)

[<Fact>]
let ``verifySignature delegates to C# WebhookVerifier`` () =
    // Verify that the F# function produces the same result as calling the C# verifier directly
    let secret = "whsec_delegate_test"
    let body = """{"test":true}"""
    let timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
    let signature = computeSignature secret timestamp body

    let fsharpResult = Webhooks.verifySignature body signature timestamp secret
    let csharpResult = SignDocsBrasil.Api.WebhookVerifier.VerifySignature(body, signature, timestamp, secret)

    Assert.Equal(csharpResult, fsharpResult)

[<Fact>]
let ``Webhooks module delete function returns correct type`` () =
    let _delete: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<unit, SignDocsError>> =
        Webhooks.delete

    Assert.True(true)

[<Fact>]
let ``Webhooks module test function returns correct type`` () =
    let _test: string -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.WebhookTestResponse, SignDocsError>> =
        Webhooks.test

    Assert.True(true)

[<Fact>]
let ``Webhooks module register function returns correct type`` () =
    let _register: SignDocsBrasil.Api.Models.RegisterWebhookRequest -> SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<SignDocsBrasil.Api.Models.RegisterWebhookResponse, SignDocsError>> =
        Webhooks.register

    Assert.True(true)

[<Fact>]
let ``Webhooks module list function returns correct type`` () =
    let _list: SignDocsBrasil.Api.SignDocsBrasilClient -> Async<Result<System.Collections.Generic.List<SignDocsBrasil.Api.Models.Webhook>, SignDocsError>> =
        Webhooks.list

    Assert.True(true)
