module SignDocsBrasil.FSharp.Tests.ClientTests

open System
open Xunit
open SignDocsBrasil.FSharp

[<Fact>]
let ``ClientConfig.defaults fills correct base URL`` () =
    let config = ClientConfig.defaults "my-client-id" (ClientSecret "my-secret")
    Assert.Equal("https://api.signdocs.com.br", config.BaseUrl)

[<Fact>]
let ``ClientConfig.defaults fills correct timeout`` () =
    let config = ClientConfig.defaults "my-client-id" (ClientSecret "my-secret")
    Assert.Equal(TimeSpan.FromSeconds(30.0), config.Timeout)

[<Fact>]
let ``ClientConfig.defaults fills correct max retries`` () =
    let config = ClientConfig.defaults "my-client-id" (ClientSecret "my-secret")
    Assert.Equal(5, config.MaxRetries)

[<Fact>]
let ``ClientConfig.defaults stores client ID`` () =
    let config = ClientConfig.defaults "test-client" (ClientSecret "s")
    Assert.Equal("test-client", config.ClientId)

[<Fact>]
let ``ClientConfig.defaults includes expected scopes`` () =
    let config = ClientConfig.defaults "my-client-id" (ClientSecret "my-secret")
    Assert.Equal(5, config.Scopes.Length)
    Assert.Contains("transactions:read", config.Scopes)
    Assert.Contains("transactions:write", config.Scopes)
    Assert.Contains("steps:write", config.Scopes)
    Assert.Contains("evidence:read", config.Scopes)
    Assert.Contains("webhooks:write", config.Scopes)

[<Fact>]
let ``ClientConfig with ClientSecret auth method`` () =
    let config = ClientConfig.defaults "my-client-id" (ClientSecret "super-secret")

    match config.Auth with
    | ClientSecret secret -> Assert.Equal("super-secret", secret)
    | PrivateKeyJwt _ -> Assert.Fail("Expected ClientSecret")

[<Fact>]
let ``ClientConfig with PrivateKeyJwt auth method`` () =
    let config = ClientConfig.defaults "my-client-id" (PrivateKeyJwt("pem-data", "kid-123"))

    match config.Auth with
    | PrivateKeyJwt(pem, kid) ->
        Assert.Equal("pem-data", pem)
        Assert.Equal("kid-123", kid)
    | ClientSecret _ -> Assert.Fail("Expected PrivateKeyJwt")

[<Fact>]
let ``ClientConfig can be customized with record update`` () =
    let config =
        { ClientConfig.defaults "my-client-id" (ClientSecret "s") with
            BaseUrl = "https://custom.example.com"
            Timeout = TimeSpan.FromSeconds(60.0)
            MaxRetries = 10 }

    Assert.Equal("https://custom.example.com", config.BaseUrl)
    Assert.Equal(TimeSpan.FromSeconds(60.0), config.Timeout)
    Assert.Equal(10, config.MaxRetries)

[<Fact>]
let ``ClientConfig scopes can be customized`` () =
    let config =
        { ClientConfig.defaults "my-client-id" (ClientSecret "s") with
            Scopes = [ "custom:scope" ] }

    Assert.Equal(1, config.Scopes.Length)
    Assert.Equal("custom:scope", config.Scopes.[0])
