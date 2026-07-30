# Changelog

## [1.8.0] - 2026-07-30

### Added

- **Envelope cancellation** — `POST /v1/envelopes/{envelopeId}/cancel` has existed since envelopes shipped and is what the Telegram bot calls, but no SDK exposed it. Consumers were left cancelling each member session by hand, which is not the same operation: it leaves the envelope's own status ACTIVE (verified against HML — an envelope whose sessions are every one CANCELLED still reports ACTIVE), costs a call per signer, and records N separate cancellations instead of one auditable terminal event.
  - Transitions every non-terminal session and its transaction to CANCELLED, then marks the envelope CANCELLED.
  - Signatures already collected are preserved and reported as `preservedSignedCount` — cancelling stops the pending signers, it never invalidates evidence already gathered.
  - Idempotent: re-cancelling returns `cancelledCount` 0 and `alreadyCancelled` true.
  - Optional `reason` is recorded in the audit trail; the API defaults it to `envelope_cancelled`.
  - Shipped in lockstep with signdocs-brasil-php 1.9.0.

### Changed

- `User-Agent` bumped to `signdocs-brasil-dotnet/1.8.0`.

## [1.7.0] - 2026-07-29

### Added

- **`signatureUrl` and `documentFormat` on the download response.** `GET /v1/transactions/{id}/download` has always returned these for non-PDF transactions, but the model parsed only `originalUrl` / `signedUrl` and silently dropped them — so there was no way to reach a detached CAdES signature through the SDK at all. Verified against HML: the API returns six fields where the model exposed four.
  - `documentFormat` is `'pdf'` or `'generic'`, derived by the API from the uploaded bytes (not the filename).
  - `signatureUrl` is the presigned URL for the detached `.p7s`, returned **instead of** `signedUrl` when `documentFormat` is `'generic'` — a non-PDF cannot carry an embedded signature.
  - Caveat worth knowing when consuming it: the API presigns that S3 key without checking that the object exists, so a non-PDF signed under a click/OTP policy still comes back with a `signatureUrl` — one that 404s on GET, because only the digital-certificate step writes a `.p7s`. Branch on the signing policy, not on the field being set.
  - Shipped in lockstep with signdocs-brasil-php 1.8.0.

## [1.6.1] - 2026-06-25

### Changed

- API-documentation link in README/package metadata now points to https://docs.signdocs.com.br (was a dead relative path).

## [1.6.0] - 2026-06-25

### Added

- `client.Verification.VerifyDocumentAsync(request)` — async method for the new `POST /v1/verify/document` endpoint. Inspects an arbitrary PDF (base64-encoded in `VerifyDocumentRequest.Content`, optional `Filename`) for embedded signatures and reports whether the document is signed, how many signatures were found, and a per-signature breakdown. Unlike the other verification methods this endpoint is **authenticated** (sends a Bearer token, requires the `verification:write` scope) and is available with **production credentials only**.
- `VerifyDocumentRequest`, `VerifyDocumentResponse`, and `DetectedSignature` models. Each `DetectedSignature` exposes `Method`, `Type` (one of `"pades"`, `"pkcs7"`, `"legacy"`, `"digital_certificate"`), `Confidence`, and the optional `SubFilter` / `Filter` PDF dictionary values.
- `Verification.verifyDocument` in the idiomatic F# wrapper, mirroring `Verification.verify` / `Verification.downloads`.

### Changed

- `User-Agent` bumped to `signdocs-brasil-dotnet/1.6.0` (the internal `SdkVersion` constant had drifted at `1.5.0` while the package shipped as `1.5.1`; both are now aligned to `1.6.0`).

## [1.5.0] - 2026-04-27

### Added

- `EnvelopeId` property on the `VerificationResponse` record — populated when the verified evidence belongs to a multi-signer envelope. Use it with `client.Verification.VerifyEnvelopeAsync(envelopeId)` for cross-signer drill-down.
- Three new `WebhookEvent` enum members:
  - `EnvelopeCreated` (`ENVELOPE.CREATED`)
  - `EnvelopeAllSigned` (`ENVELOPE.ALL_SIGNED`)
  - `EnvelopeExpired` (`ENVELOPE.EXPIRED`)

### Changed

- `User-Agent` bumped to `signdocs-brasil-dotnet/1.5.0`.

## [1.4.1] - 2026-04-27

### Fixed

- `WebhookTestResponse` shape — was `{deliveryId, status, statusCode}`, now matches the API spec `{webhookId, testDelivery: {httpStatus, success, error?, timestamp}}`. The typed wrapper for `client.Webhooks.TestAsync()` was returning all-empty fields against the live HML API; consumers will now see the real delivery result. New nested record `WebhookTestDelivery` exposes `HttpStatus`, `Success`, `Timestamp`, and nullable `Error`.

### Changed

- User-Agent bumped to `signdocs-brasil-dotnet/1.4.1`.

## [1.4.0] - 2026-04-23

### Fixed (BREAKING IF YOU SOMEHOW USED 1.x SUCCESSFULLY)

- **Realigned every signing-session and envelope model class to match the actual API schema.** Releases 1.0.0 through 1.3.0 shipped with hand-written models that didn't match what the server validates: `CreateSigningSessionRequest` used legacy fields (`Name`, `Type`, `Signers[]`, `Documents[]`, `CallbackUrl`, `RedirectUrl`, `BrandingId`) that the API has never accepted, so any call would have returned 400 Bad Request. The TypeScript / Python / Go SDKs already used the correct shape; this brings .NET into alignment.
- Affected classes: `CreateSigningSessionRequest`, `SigningSession`, `SigningSessionStatus`, `CreateEnvelopeRequest`, `AddEnvelopeSessionRequest`, `EnvelopeSession`. The new shape uses `Purpose`, `Policy`, `Signer`, `Document`, `ReturnUrl`, `CancelUrl`, `Metadata`, `Locale`, `ExpiresInMinutes`, `Appearance` — matching the OpenAPI spec.
- `Policy` and `Signer` (top-level models) were already correct and are reused unchanged. `Envelope`, `EnvelopeSessionSummary`, `EnvelopeDetail` were already correct and are unchanged.

### Added

- `Owner` model — optional requester identity (`Email`, `Name`) on `CreateSigningSessionRequest` and `CreateEnvelopeRequest`. When provided, SignDocs automatically emails each signer an invitation with their signing URL (when `Signer.Email` differs from `Owner.Email`, case-insensitive) and emails the owner a completion notification per signer completion (plus a final "all signed" message for envelopes). Omit to keep the traditional behavior.
- `InviteSent` (`bool?`) on `SigningSession` and `EnvelopeSession` response models. Populated by the API when an invitation email was dispatched.

### Changed

- `User-Agent` bumped to `signdocs-brasil-dotnet/1.4.0`.

## [1.3.0] - 2026-04-20

### Fixed

- `WebhooksResource.ListAsync()` now correctly returns `List<Webhook>`. Previously `System.Text.Json` failed to deserialize the API's `{"webhooks":[...],"count":N}` envelope into `List<Webhook>` (array vs. object mismatch). The method now deserializes into an internal `WebhookListEnvelope` DTO and returns its inner list.

### Added

- `SignDocsBrasil.Api.TokenCache` namespace: `ITokenCache` interface, `CachedToken` record, `InMemoryTokenCache` default implementation (thread-safe via `ConcurrentDictionary`), and `TokenCacheKeys.Derive` helper. Inject via `SignDocsBrasilClientOptions.TokenCache` to share OAuth tokens across processes/pods (Redis, distributed cache). Default preserves pre-1.3 single-process behavior.
- `SignDocsBrasil.Api.ResponseMetadata` — captures `RateLimit-*`, `Deprecation`, `Sunset`, and `X-Request-Id` / `X-SignDocs-Request-Id` headers from every API response. Register an observer via `SignDocsBrasilClientOptions.OnResponse`. RFC 8594 parser accepts both `@<unix-seconds>` and IMF-fixdate forms.
- Webhook event names for the NT65 INSS consignado flow:
  - `STEP.PURPOSE_DISCLOSURE_SENT` — purpose-disclosure notification delivered to the beneficiary
  - `TRANSACTION.DEADLINE_APPROACHING` — ≤2 business days remaining until the INSS submission deadline

### Changed

- `SignDocsBrasil.Api.Internal.AuthHandler` promoted from `internal sealed` to `public` (non-sealed). Consumers can now inject custom `ITokenCache` implementations without reflection. This is strictly more permissive — no existing consumer breaks.
- User-Agent bumped to `1.3.0`.

## [1.2.0] - 2026-04-14

### Added

- `client.Verification.VerifyEnvelopeAsync(envelopeId)` — public async method for the new `GET /v1/verify/envelope/{envelopeId}` endpoint. Returns envelope status, signers list (each with `EvidenceId` for drill-down via `VerifyAsync()`), and consolidated download URLs.
- `EnvelopeVerificationResponse`, `EnvelopeVerificationSigner`, and `EnvelopeVerificationDownloads` records. For non-PDF envelopes signed with digital certificates, `Downloads.ConsolidatedSignature` exposes a single PKCS#7 / CMS detached `.p7s` containing every signer's `SignerInfo`. For PDF envelopes, `Downloads.CombinedSignedPdf` exposes the merged PDF.
- `VerificationSigner.CpfCnpj` and `VerificationResponse.TenantCnpj` fields (previously returned by the API but not modeled by the SDK).
- `Downloads.OriginalDocument` and `Downloads.SignedSignature` fields on `VerificationDownloadsResponse` (previously undocumented), matching the real shape the API returns.

### Changed

- `Downloads.SignedSignature` is now `null` when the evidence belongs to a multi-signer envelope (the API omits the field). For standalone signing sessions (single-signer non-PDF with digital certificate) the field is still populated. To retrieve the consolidated `.p7s` for an envelope, use `client.Verification.VerifyEnvelopeAsync()` instead.

### Removed

- `Downloads.SignedPdf` — the field was modeled by the SDK but never actually returned by the API. No real-world consumer could have depended on it.

## [1.1.0] - 2026-03-27

### Added

- Envelopes resource (`client.Envelopes`): CreateAsync, GetAsync, AddSessionAsync, CombinedStampAsync — multi-signer workflows with parallel or sequential signing
- New models: CreateEnvelopeRequest, Envelope, AddEnvelopeSessionRequest, EnvelopeSession, EnvelopeSessionSummary, EnvelopeDetail, EnvelopeCombinedStampResponse

## 1.0.0 (2026-03-04)

### Added
- Initial release of the SignDocsBrasil .NET SDK
- Full API coverage: Transactions, Documents, Steps, Signing, Evidence, Verification, Users, Webhooks, DocumentGroups
- OAuth2 authentication with client_secret and private_key_jwt (ES256)
- Automatic retry with exponential backoff for 429, 500, 503
- Auto-pagination via IAsyncEnumerable<T>
- Webhook signature verification (HMAC-SHA256)
- Idiomatic F# wrapper (SignDocsBrasil.FSharp) with Result-based error handling
- Per-request timeout support
- Custom HttpClient injection
- ILogger integration for request/response logging
