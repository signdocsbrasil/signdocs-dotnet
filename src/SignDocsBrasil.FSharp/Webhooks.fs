namespace SignDocsBrasil.FSharp

open SignDocsBrasil.Api
open SignDocsBrasil.Api.Models

module Webhooks =
    /// Registers a new webhook.
    let register (request: RegisterWebhookRequest) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Webhooks.RegisterAsync(request))

    /// Lists all registered webhooks.
    let list (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Webhooks.ListAsync())

    /// Deletes a webhook by ID.
    let delete (webhookId: string) (client: SignDocsBrasilClient) =
        tryCallTaskAsync (fun () -> client.Webhooks.DeleteAsync(webhookId))

    /// Sends a test event to a webhook.
    let test (webhookId: string) (client: SignDocsBrasilClient) =
        tryCallAsync (fun () -> client.Webhooks.TestAsync(webhookId))

    /// Verifies a webhook signature using HMAC-SHA256.
    let verifySignature (body: string) (signatureHeader: string) (timestampHeader: string) (secret: string) =
        WebhookVerifier.VerifySignature(body, signatureHeader, timestampHeader, secret)
