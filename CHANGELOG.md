# Changelog

## [1.13.0] - 2026-09-01

### Added

- **Capture metrics on every enrollment response**, not just on a dry run.
  `quality` (brightness, sharpness), `pose`, `faceCoverage` and `warnings` now
  come back from a **successful** enrolment too.
  - This closes the gap that mattered: `faceConfidence` answers *"is this a
    face?"*, and a dark, blurred photo scores **99.99** on it. Anyone enrolling
    one user at a time got that reassuring number and no other signal, while
    batch callers using `dryRun` saw the whole picture.
  - The photo is stored either way; knowing it is weak now beats finding out
    from a failed signature three months later. Costs no extra Rekognition
    call — the data was already being fetched and discarded.
- **`dryRun` on the single enrollment endpoint**, for symmetry with the batch,
  plus an `inspect` helper that sets it for you. Same verdict, from the same
  code, so a photo cannot be judged differently depending on which endpoint you
  asked.

## [1.12.0] - 2026-09-01

### Added

- **Batch enrollment** — `POST /v1/users/enrollments`, up to 25 users per
  request. No SDK exposed this endpoint before; the route itself only went live
  the day prior.
  - The documented cap is 25 rows, but the binding limit is the request body
    (~6 MB, and base64 inflates each photo by a third). At 640x640 all 25 slots
    fit; at full camera resolution you get about 8.
  - **Partial success returns `200`.** One unusable photo must not reject the
    other twenty-four, so every row reports its own outcome. Read `results`, not
    the HTTP status, or a half-failed batch looks like a success.
- **`dryRun` — reference photo screening.** Inspects every row and writes
  nothing: no image reaches storage, no record is created, and the 90-day
  retention clock never starts.
  - It exists because Rekognition's confidence answers *"is this a face?"*, not
    *"is this a good reference?"* Measured: a photo at brightness 15 and
    sharpness 13 enrols successfully at 99.99 confidence, then fails face
    matching months later, one user at a time.
  - Three states, since "can I enrol this?" has three answers. **`marginal` is
    the one to act on** — it enrols without complaint today and is exactly what
    becomes a rejected signature later.
  - Rows carry `quality` (brightness, sharpness), `pose` (yaw, pitch, roll),
    `faceCoverage` and `warnings`: `LOW_BRIGHTNESS`, `LOW_SHARPNESS`,
    `FACE_TOO_SMALL`, `HEAD_TURNED`.
  - Costs the same one Rekognition call per row that enrolling costs. The
    saving is not money — it is not storing biometrics already judged unusable.

## [1.11.0] - 2026-08-31

### Added

- **Enrollment read and erase** — `GET` and `DELETE` on
  `/v1/users/{userExternalId}/enrollment`. Only `PUT` was exposed before, so
  neither the re-enrolment sweep nor LGPD art. 18 erasure was reachable from
  the SDK.
  - `GET` reports `expiresAt` / `expired`. The reference image is hard-deleted
    by lifecycle 90 days after enrolment and the record outlives it by a grace
    window *so that* this flag can be found in time. Sweep inside that window:
    once it closes the record goes too and the route answers `404`, which is
    indistinguishable from "never enrolled". Miss it and the expiry surfaces as
    a `422` in the middle of a signature.
  - `DELETE` destroys every stored version of the reference image, not just the
    current one — `versionsDeleted` reports how many.
- **`ENROLLMENT.EXPIRING` / `ENROLLMENT.EXPIRED`** webhook event types.
- **Per-request biometric thresholds** — `policy.minSimilarity` and
  `policy.minLivenessConfidence` let a transaction demand more confidence than
  the account default. They only tighten: a value below the tenant minimum is
  rejected with `400` naming the current floor rather than silently ignored.
  Percentages (`95`) and fractions (`0.95`) both pass through untouched.
- **Advance surface brought in line with the API** — the `confirm_signer` and
  `complete_document_photo` actions, plus `cpfCnpj`, `documentImage`,
  `documentType`, `deviceInfo` and the four sandbox scores
  (`sandboxSimilarity`, `sandboxLivenessConfidence`, `sandboxBrightness`,
  `sandboxSharpness`). The document-photo fallback flow was previously not
  reachable from any SDK.
- **`errorCode` / `errorDetail` / `retryable` / `fallback` on the advance
  response.** This is the one to read if you integrate biometrics: a rejected
  step returns **200** with the session still `ACTIVE` and the reason in the
  body, not as an HTTP error. Code that only branches on the status — or only
  catches exceptions — reads a rejection as a success. Emitted today:
  `BIOMETRIC_MATCH_FAILED`, `LIVENESS_NOT_COMPLETED`, `DOCUMENT_QUALITY_LOW`,
  `DOCUMENT_MATCH_FAILED` and the `SERPRO_*` family.
- **`referenceImage` on session creation** — a per-transaction reference face,
  which allows signing without a prior enrolment.

## [1.10.0] - 2026-08-20

### Added

- **`SigningSessions.LinkAsync(sessionId)`** — `POST /v1/signing-sessions/{sessionId}/link`. The endpoint has
  been in the API and documented in the OpenAPI spec all along, but no SDK in any
  language exposed it, so there was no supported way to recover a signing link
  once the create response was gone.
  - A signing link is single-use: after the signer finishes — or the embed token
    is otherwise consumed — reopening the same URL returns
    `401 Embed token has been consumed`. This mints a new one **without creating
    another transaction and without consuming quota**.
  - Works for standalone and envelope sessions alike.
  - The session must be `ACTIVE`. A completed or cancelled one returns 409: a
    link to a finished session would authenticate nothing. Reach the signed
    document through the envelope's combined stamp or the transaction download
    instead.
  - `expiresAt` is inherited from the original session and is **not** extended.
  - Sends no idempotency key, deliberately. A retry must mint a fresh URL, not
    replay one that has already been consumed.
  - **Authorises the tenant, not the end user.** The API cannot tell which of
    your users is entitled to a given link, so an application whose users share
    one tenant has to establish that itself before calling — otherwise this is a
    way for one user to obtain another's signing credential.
- `MintSigningLinkResponse` model (`SessionId`, `TransactionId`, `Url`, `ExpiresAt`, `ExpiresIn`).

### Fixed

- **The `User-Agent` reported a version nobody was running.** 1.9.0 shipped reporting `signdocs-brasil-dotnet/1.8.0`, guarded by a test named `SdkVersion_Is1_6_1` that asserted 1.8.0 — the name and the assertion had already disagreed for two releases. The version
  constant now moves with the package, and a test compares it against
  the assembly version so a release that forgets it fails instead of shipping.

## [1.9.0] - 2026-08-20

### Fixed

- **`addSession`/`verifyDocument` sent no idempotency key** while the client
  retries 429/500/503, so a 500 on an add-session became a second signer, a
  second quota charge and a second invitation, and a retried `verifyDocument`
  paid the metered verification quota twice for an identical result. Pass a
  distinct key per signer: the API scopes its cache by key and resolved path,
  and all signers on an envelope share that path.

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
