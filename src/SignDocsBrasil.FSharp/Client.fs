namespace SignDocsBrasil.FSharp

open System
open SignDocsBrasil.Api

/// Authentication method for the SDK.
type AuthMethod =
    | ClientSecret of secret: string
    | PrivateKeyJwt of pem: string * kid: string

/// Configuration for the SignDocsBrasil client.
type ClientConfig =
    { ClientId: string
      Auth: AuthMethod
      BaseUrl: string
      Timeout: TimeSpan
      MaxRetries: int
      Scopes: string list }

module ClientConfig =
    /// Creates a default configuration with standard values.
    let defaults (clientId: string) (auth: AuthMethod) : ClientConfig =
        { ClientId = clientId
          Auth = auth
          BaseUrl = "https://api.signdocs.com.br"
          Timeout = TimeSpan.FromSeconds(30.0)
          MaxRetries = 5
          Scopes =
            [ "transactions:read"
              "transactions:write"
              "steps:write"
              "evidence:read"
              "webhooks:write" ] }

module Client =
    /// Creates a SignDocsBrasilClient from an F# ClientConfig.
    let create (config: ClientConfig) : SignDocsBrasilClient =
        let builder =
            SignDocsBrasilClient
                .CreateBuilder()
                .ClientId(config.ClientId)
                .BaseUrl(config.BaseUrl)
                .Timeout(config.Timeout)
                .MaxRetries(config.MaxRetries)
                .Scopes(config.Scopes |> Array.ofList)

        let builder =
            match config.Auth with
            | ClientSecret secret -> builder.ClientSecret(secret)
            | PrivateKeyJwt(pem, kid) -> builder.PrivateKey(pem).Kid(kid)

        builder.Build()
