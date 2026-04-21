# Changelog

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
