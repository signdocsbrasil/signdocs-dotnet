# SignDocsBrasil .NET SDK

Official .NET SDK for the SignDocs Brasil electronic signature API. Provides full coverage of the SignDocs Brasil REST API with idiomatic C# and F# support, including OAuth2 authentication, automatic retries, auto-pagination, and webhook verification.

## Requirements

- **.NET 8.0** or later

## Installation

Install the C# SDK via NuGet:

```bash
dotnet add package SignDocsBrasil.Api
```

For idiomatic F# support with `Result`-based error handling:

```bash
dotnet add package SignDocsBrasil.FSharp
```

## Quick Start

### C#

```csharp
using SignDocsBrasil.Api;
using SignDocsBrasil.Api.Models;

using var client = SignDocsBrasilClient.CreateBuilder()
    .ClientId("your-client-id")
    .ClientSecret("your-client-secret")
    .Build();

var transaction = await client.Transactions.CreateAsync(new CreateTransactionRequest
{
    Purpose = "DOCUMENT_SIGNATURE",
    Policy = new Policy { Profile = "CLICK_ONLY" },
    Signer = new Signer
    {
        Name = "João Silva",
        Email = "joao@example.com",
        Cpf = "12345678901"
    }
});

Console.WriteLine($"Transaction ID: {transaction?.TransactionId}");
```

### F#

```fsharp
open SignDocsBrasil.FSharp
open SignDocsBrasil.Api.Models

let config =
    ClientConfig.defaults "your-client-id" (ClientSecret "your-client-secret")

use client = Client.create config

let result =
    client
    |> Transactions.create (CreateTransactionRequest(Purpose = "DOCUMENT_SIGNATURE"))
    |> Async.RunSynchronously

match result with
| Ok tx -> printfn "Transaction ID: %s" tx.TransactionId
| Error err ->
    match err with
    | BadRequest pd -> printfn "Bad request: %s" pd.Detail
    | RateLimit (_, retryAfter) -> printfn "Rate limited, retry after: %A" retryAfter
    | _ -> printfn "Error: %A" err
```

## Available Resources

| Resource | Methods |
|----------|---------|
| `Health` | `CheckAsync()`, `HistoryAsync()` |
| `Transactions` | `CreateAsync()`, `ListAsync()`, `GetAsync()`, `CancelAsync()`, `FinalizeAsync()`, `ListAutoPaginateAsync()` |
| `Documents` | `UploadAsync()`, `PresignAsync()`, `ConfirmAsync()`, `DownloadAsync()` |
| `Steps` | `ListAsync()`, `StartAsync()`, `CompleteAsync()` |
| `Signing` | `PrepareAsync()`, `CompleteAsync()` |
| `Evidence` | `GetAsync()` |
| `Verification` | `VerifyAsync()`, `DownloadsAsync()` |
| `Users` | `EnrollAsync()` |
| `Webhooks` | `RegisterAsync()`, `ListAsync()`, `DeleteAsync()`, `TestAsync()` |
| `SigningSessions` | `CreateAsync()`, `GetStatusAsync()`, `CancelAsync()`, `ListAsync()`, `WaitForCompletionAsync()` |
| `Envelopes` | `CreateAsync()`, `GetAsync()`, `AddSessionAsync()`, `CombinedStampAsync()` |
| `DocumentGroups` | `CombinedStampAsync()` |

## Envelopes (Multi-Signer)

```csharp
var envelope = await client.Envelopes.CreateAsync(new CreateEnvelopeRequest
{
    SigningMode = "PARALLEL",
    TotalSigners = 2,
    DocumentContent = pdfBase64,
    DocumentFilename = "contrato.pdf",
});

var session1 = await client.Envelopes.AddSessionAsync(envelope.EnvelopeId, new AddEnvelopeSessionRequest
{
    SignerName = "João Silva",
    SignerEmail = "joao@example.com",
    PolicyProfile = "CLICK_ONLY",
    SignerIndex = 1,
});

var session2 = await client.Envelopes.AddSessionAsync(envelope.EnvelopeId, new AddEnvelopeSessionRequest
{
    SignerName = "Maria Santos",
    SignerEmail = "maria@example.com",
    PolicyProfile = "CLICK_ONLY",
    SignerIndex = 2,
});

Console.WriteLine($"{session1.Url} {session2.Url}");
```

## Error Handling

### C#

The SDK throws typed exceptions that you can catch and handle:

```csharp
using SignDocsBrasil.Api;
using SignDocsBrasil.Api.Exceptions;

try
{
    var tx = await client.Transactions.CreateAsync(request);
}
catch (SignDocsValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.ProblemDetails.Detail}");
    foreach (var error in ex.ProblemDetails.Errors)
    {
        Console.WriteLine($"  {error.Field}: {error.Message}");
    }
}
catch (SignDocsRateLimitException ex)
{
    Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter}");
}
catch (SignDocsApiException ex)
{
    Console.WriteLine($"API error {ex.StatusCode}: {ex.Message}");
}
```

### F#

The F# wrapper returns `Result<'T, ApiError>` instead of throwing exceptions:

```fsharp
let result =
    client
    |> Transactions.create request
    |> Async.RunSynchronously

match result with
| Ok tx ->
    printfn "Created: %s" tx.TransactionId
| Error err ->
    match err with
    | BadRequest pd -> printfn "Validation: %s" pd.Detail
    | NotFound -> printfn "Not found"
    | RateLimit (pd, retryAfter) -> printfn "Rate limited, retry after %A" retryAfter
    | ServerError (status, pd) -> printfn "Server error %d: %s" status pd.Detail
    | NetworkError ex -> printfn "Network error: %s" ex.Message
```

## Pagination

### C# (IAsyncEnumerable)

Use `ListAutoPaginateAsync()` to iterate through all pages automatically:

```csharp
await foreach (var tx in client.Transactions.ListAutoPaginateAsync())
{
    Console.WriteLine($"{tx.TransactionId}: {tx.Status}");
}
```

Or retrieve a single page with `ListAsync()`:

```csharp
var page = await client.Transactions.ListAsync(new ListTransactionsRequest
{
    Page = 1,
    PageSize = 25
});

foreach (var tx in page.Items)
{
    Console.WriteLine($"{tx.TransactionId}: {tx.Status}");
}
```

### F# (AsyncSeq)

```fsharp
let! transactions =
    client
    |> Transactions.listAutoPaginate ()
    |> AsyncSeq.toListAsync

transactions |> List.iter (fun tx ->
    printfn "%s: %s" tx.TransactionId tx.Status)
```

## Webhook Verification

Verify incoming webhook signatures to ensure authenticity.

### C#

```csharp
using SignDocsBrasil.Api.Webhooks;

var verifier = new WebhookVerifier("your-webhook-secret");

bool isValid = verifier.Verify(
    payload: requestBody,
    signature: request.Headers["X-SignDocs-Signature"],
    timestamp: request.Headers["X-SignDocs-Timestamp"]
);

if (isValid)
{
    var webhookEvent = verifier.Parse(requestBody);
    Console.WriteLine($"Event: {webhookEvent.Type}");
}
```

### F#

```fsharp
open SignDocsBrasil.FSharp.Webhooks

let result =
    Webhook.verify "your-webhook-secret" requestBody signature timestamp

match result with
| Ok event -> printfn "Event: %s" event.Type
| Error err -> printfn "Invalid webhook: %s" err
```

## Advanced Configuration

### Custom HttpClient

Inject your own `HttpClient` for custom transport settings:

```csharp
var httpClient = new HttpClient(new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(2)
});

using var client = SignDocsBrasilClient.CreateBuilder()
    .ClientId("your-client-id")
    .ClientSecret("your-client-secret")
    .HttpClient(httpClient)
    .Build();
```

### Logging

Integrate with `Microsoft.Extensions.Logging`:

```csharp
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

using var client = SignDocsBrasilClient.CreateBuilder()
    .ClientId("your-client-id")
    .ClientSecret("your-client-secret")
    .Logger(loggerFactory)
    .Build();
```

### Per-Request Timeout

Override the default timeout for individual requests:

```csharp
var tx = await client.Transactions.GetAsync(
    transactionId,
    options: new RequestOptions { Timeout = TimeSpan.FromSeconds(5) }
);
```

### Scopes

Request specific OAuth2 scopes:

```csharp
using var client = SignDocsBrasilClient.CreateBuilder()
    .ClientId("your-client-id")
    .ClientSecret("your-client-secret")
    .Scopes("transactions:read", "transactions:write", "documents:write")
    .Build();
```

### Private Key JWT Authentication (ES256)

Use `private_key_jwt` instead of `client_secret` for enhanced security:

```csharp
using var client = SignDocsBrasilClient.CreateBuilder()
    .ClientId("your-client-id")
    .PrivateKeyPem(File.ReadAllText("private-key.pem"))
    .Build();
```

## License

MIT
