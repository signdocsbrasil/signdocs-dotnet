# Changelog

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
