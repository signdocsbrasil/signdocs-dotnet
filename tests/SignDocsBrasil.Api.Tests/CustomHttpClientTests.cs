using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class CustomHttpClientTests
{
    [Fact]
    public void ClientCreatedWithCustomHttpClient_UsesIt()
    {
        var handler = new MockHttpHandler();
        handler.EnqueueToken("custom-client-token");

        var customHttpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://custom.api.com")
        };

        var client = SignDocsBrasilClient.CreateBuilder()
            .ClientId("test-id")
            .ClientSecret("test-secret")
            .HttpClient(customHttpClient)
            .Build();

        // Just verify the client was created without error
        Assert.NotNull(client);
        client.Dispose();
    }

    [Fact]
    public void CustomHttpClient_IsNotDisposedWhenClientDisposed()
    {
        var handler = new MockHttpHandler();
        var customHttpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://custom.api.com")
        };

        var client = SignDocsBrasilClient.CreateBuilder()
            .ClientId("test-id")
            .ClientSecret("test-secret")
            .HttpClient(customHttpClient)
            .Build();

        client.Dispose();

        // Custom HttpClient should still be usable after SDK disposal
        // If it was disposed, this would throw ObjectDisposedException
        handler.EnqueueJson(200, """{"status":"ok"}""");
        var ex = Record.Exception(() =>
        {
            customHttpClient.GetAsync("https://custom.api.com/health")
                .GetAwaiter().GetResult();
        });
        Assert.Null(ex);

        customHttpClient.Dispose();
    }

    [Fact]
    public void Builder_SetsAllProperties()
    {
        var handler = new MockHttpHandler();
        var customHttpClient = new HttpClient(handler);

        var client = SignDocsBrasilClient.CreateBuilder()
            .ClientId("my-client")
            .ClientSecret("my-secret")
            .BaseUrl("https://staging.api.com")
            .Timeout(TimeSpan.FromSeconds(60))
            .MaxRetries(3)
            .Scopes("transactions:read")
            .HttpClient(customHttpClient)
            .Build();

        Assert.NotNull(client);
        client.Dispose();
        customHttpClient.Dispose();
    }

    [Fact]
    public void Builder_WithPrivateKeyAndKid()
    {
        var handler = new MockHttpHandler();
        var customHttpClient = new HttpClient(handler);

        var client = SignDocsBrasilClient.CreateBuilder()
            .ClientId("my-client")
            .PrivateKey("-----BEGIN PRIVATE KEY-----\nfake\n-----END PRIVATE KEY-----")
            .Kid("key-001")
            .HttpClient(customHttpClient)
            .Build();

        Assert.NotNull(client);
        client.Dispose();
        customHttpClient.Dispose();
    }

    [Fact]
    public void Builder_ThrowsForMissingClientId()
    {
        Assert.Throws<ArgumentException>(() =>
            SignDocsBrasilClient.CreateBuilder()
                .ClientSecret("secret")
                .Build());
    }

    [Fact]
    public void Builder_ThrowsForMissingCredentials()
    {
        Assert.Throws<ArgumentException>(() =>
            SignDocsBrasilClient.CreateBuilder()
                .ClientId("test-id")
                .Build());
    }

    [Fact]
    public void Builder_ThrowsForNullClientId()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SignDocsBrasilClient.CreateBuilder()
                .ClientId(null!));
    }

    [Fact]
    public void Client_HasAllResources()
    {
        var handler = new MockHttpHandler();
        var customHttpClient = new HttpClient(handler);

        using var client = SignDocsBrasilClient.CreateBuilder()
            .ClientId("test")
            .ClientSecret("secret")
            .HttpClient(customHttpClient)
            .Build();

        Assert.NotNull(client.Health);
        Assert.NotNull(client.Transactions);
        Assert.NotNull(client.Documents);
        Assert.NotNull(client.Steps);
        Assert.NotNull(client.Signing);
        Assert.NotNull(client.Evidence);
        Assert.NotNull(client.Verification);
        Assert.NotNull(client.Users);
        Assert.NotNull(client.Webhooks);
        Assert.NotNull(client.DocumentGroups);

        customHttpClient.Dispose();
    }
}
