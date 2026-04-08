using System.Text.Json;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class PerRequestTimeoutTests : IDisposable
{
    private readonly SignDocsHttpClient _client;
    private readonly MockHttpHandler _handler;

    public PerRequestTimeoutTests()
    {
        (_client, _handler) = TestClientFactory.Create();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task RequestWithCustomTimeout_CompletesSuccessfully()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        var result = await _client.RequestAsync<JsonElement>(
            HttpMethod.Get,
            "/v1/test",
            timeout: TimeSpan.FromSeconds(10));

        Assert.True(result.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public async Task RequestWithoutTimeout_UsesDefault()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        // No timeout specified - uses the default from constructor
        var result = await _client.RequestAsync<JsonElement>(
            HttpMethod.Get,
            "/v1/test");

        Assert.True(result.GetProperty("ok").GetBoolean());
    }

    [Fact]
    public async Task ResourceMethod_AcceptsTimeout()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-create.json");
        _handler.EnqueueJson(201, body);

        var resource = new SignDocsBrasil.Api.Resources.TransactionsResource(_client);
        var request = new SignDocsBrasil.Api.Models.CreateTransactionRequest
        {
            Purpose = "DOCUMENT_SIGNATURE"
        };

        var result = await resource.CreateAsync(request, timeout: TimeSpan.FromSeconds(5));

        Assert.NotNull(result);
        Assert.Equal("tx-uuid-001", result!.TransactionId);
    }

    [Fact]
    public async Task GetRequest_AcceptsTimeout()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-get.json");
        _handler.EnqueueJson(200, body);

        var resource = new SignDocsBrasil.Api.Resources.TransactionsResource(_client);
        var result = await resource.GetAsync("tx-uuid-001", timeout: TimeSpan.FromSeconds(15));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ListRequest_AcceptsTimeout()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"transactions":[],"count":0}""");

        var resource = new SignDocsBrasil.Api.Resources.TransactionsResource(_client);
        var result = await resource.ListAsync(timeout: TimeSpan.FromSeconds(20));

        Assert.NotNull(result);
    }

    [Fact]
    public async Task NoAuthRequest_AcceptsTimeout()
    {
        string body = FixtureLoader.LoadResponseBody("health-check.json");
        _handler.EnqueueJson(200, body);

        var resource = new SignDocsBrasil.Api.Resources.HealthResource(_client);
        var result = await resource.CheckAsync(timeout: TimeSpan.FromSeconds(5));

        Assert.NotNull(result);
        Assert.Equal("healthy", result!.Status);
    }

    [Fact]
    public async Task DeleteRequest_AcceptsTimeout()
    {
        _handler.EnqueueToken();
        _handler.EnqueueNoContent();

        var resource = new SignDocsBrasil.Api.Resources.WebhooksResource(_client);
        await resource.DeleteAsync("wh-001", timeout: TimeSpan.FromSeconds(10));

        Assert.Equal(2, _handler.Requests.Count);
    }

    [Fact]
    public async Task FinalizeRequest_AcceptsTimeout()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"transactionId":"tx-001","status":"COMPLETED"}""");

        var resource = new SignDocsBrasil.Api.Resources.TransactionsResource(_client);
        var result = await resource.FinalizeAsync("tx-001", timeout: TimeSpan.FromSeconds(30));

        Assert.NotNull(result);
    }
}
