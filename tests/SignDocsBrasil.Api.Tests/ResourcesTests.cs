using System.Text.Json;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;
using SignDocsBrasil.Api.Resources;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class ResourcesTests : IDisposable
{
    private readonly SignDocsHttpClient _client;
    private readonly MockHttpHandler _handler;

    public ResourcesTests()
    {
        (_client, _handler) = TestClientFactory.Create();
    }

    public void Dispose() => _client.Dispose();

    // --- Health ---

    [Fact]
    public async Task Health_Check_ReturnsHealthCheckResponse()
    {
        string body = FixtureLoader.LoadResponseBody("health-check.json");
        _handler.EnqueueJson(200, body);

        var resource = new HealthResource(_client);
        HealthCheckResponse? result = await resource.CheckAsync();

        Assert.NotNull(result);
        Assert.Equal("healthy", result!.Status);
        Assert.Equal("1.0.0", result.Version);
    }

    [Fact]
    public async Task Health_Check_NoAuthHeaderSent()
    {
        string body = FixtureLoader.LoadResponseBody("health-check.json");
        _handler.EnqueueJson(200, body);

        var resource = new HealthResource(_client);
        await resource.CheckAsync();

        // Only 1 request (no token request)
        Assert.Single(_handler.Requests);
        Assert.False(_handler.Requests[0].Headers.Contains("Authorization"));
    }

    [Fact]
    public async Task Health_Check_HasServicesInfo()
    {
        string body = FixtureLoader.LoadResponseBody("health-check.json");
        _handler.EnqueueJson(200, body);

        var resource = new HealthResource(_client);
        HealthCheckResponse? result = await resource.CheckAsync();

        Assert.NotNull(result!.Services);
        Assert.Contains("dynamodb", result.Services!.Keys);
        Assert.Contains("s3", result.Services!.Keys);
        Assert.Contains("cognito", result.Services!.Keys);
    }

    [Fact]
    public async Task Health_History_ReturnsHistoryResponse()
    {
        _handler.EnqueueJson(200, """{"entries":[]}""");

        var resource = new HealthResource(_client);
        HealthHistoryResponse? result = await resource.HistoryAsync();

        Assert.NotNull(result);
        Assert.Empty(result!.Entries!);
    }

    // --- Webhooks ---

    [Fact]
    public async Task Webhooks_Register_ReturnsResponse()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("webhooks-register.json");
        _handler.EnqueueJson(201, body);

        var resource = new WebhooksResource(_client);
        var request = new RegisterWebhookRequest
        {
            Url = "https://example.com/webhooks/signdocs",
            Events = new List<string> { "TRANSACTION.COMPLETED" }
        };

        RegisterWebhookResponse? result = await resource.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("wh-uuid-001", result!.WebhookId);
        Assert.Equal("ACTIVE", result.Status);
    }

    [Fact]
    public async Task Webhooks_Register_SetsIdempotencyKey()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("webhooks-register.json");
        _handler.EnqueueJson(201, body);

        var resource = new WebhooksResource(_client);
        var request = new RegisterWebhookRequest
        {
            Url = "https://example.com/hook",
            Events = new List<string> { "TRANSACTION.COMPLETED" }
        };

        await resource.RegisterAsync(request, idempotencyKey: "idem-key-001");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal("idem-key-001",
            apiRequest.Headers.GetValues("X-Idempotency-Key").First());
    }

    [Fact]
    public async Task Webhooks_Register_HasSecret()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("webhooks-register.json");
        _handler.EnqueueJson(201, body);

        var resource = new WebhooksResource(_client);
        var request = new RegisterWebhookRequest
        {
            Url = "https://example.com/hook",
            Events = new List<string> { "TRANSACTION.COMPLETED" }
        };

        RegisterWebhookResponse? result = await resource.RegisterAsync(request);

        Assert.NotNull(result!.Secret);
        Assert.StartsWith("whsec_", result.Secret!);
    }

    [Fact]
    public async Task Webhooks_Delete_CompletesWithoutError()
    {
        _handler.EnqueueToken();
        _handler.EnqueueNoContent();

        var resource = new WebhooksResource(_client);
        await resource.DeleteAsync("wh-uuid-001");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal(HttpMethod.Delete, apiRequest.Method);
        Assert.EndsWith("/v1/webhooks/wh-uuid-001", apiRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Webhooks_List_ReturnsListOfWebhooks()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """[{"webhookId":"wh-001","url":"https://ex.com","events":["TRANSACTION.COMPLETED"],"status":"ACTIVE","createdAt":"2024-01-01T00:00:00Z"}]""");

        var resource = new WebhooksResource(_client);
        List<Webhook>? result = await resource.ListAsync();

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("wh-001", result[0].WebhookId);
    }

    [Fact]
    public async Task Webhooks_Test_ReturnsTestResponse()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"deliveryId":"dlv-001","status":"DELIVERED","statusCode":200}""");

        var resource = new WebhooksResource(_client);
        WebhookTestResponse? result = await resource.TestAsync("wh-uuid-001");

        Assert.NotNull(result);
        Assert.Equal("dlv-001", result!.DeliveryId);
        Assert.Equal(200, result.StatusCode);
    }

    // --- Users ---

    [Fact]
    public async Task Users_Enroll_UsesPutMethod()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("users-enroll.json");
        _handler.EnqueueJson(200, body);

        var resource = new UsersResource(_client);
        var request = new EnrollUserRequest
        {
            Image = "/9j/4AAQSkZJRg...",
            Cpf = "12345678901",
            Source = "BASE64_IMAGE"
        };

        await resource.EnrollAsync("user-ext-001", request);

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal(HttpMethod.Put, apiRequest.Method);
        Assert.EndsWith("/v1/users/user-ext-001/enrollment", apiRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Users_Enroll_ReturnsResponse()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("users-enroll.json");
        _handler.EnqueueJson(200, body);

        var resource = new UsersResource(_client);
        var request = new EnrollUserRequest();

        EnrollUserResponse? result = await resource.EnrollAsync("user-ext-001", request);

        Assert.NotNull(result);
        Assert.Equal("user-ext-001", result!.UserExternalId);
    }

    // --- Verification ---

    [Fact]
    public async Task Verification_Verify_ReturnsResponse()
    {
        string body = FixtureLoader.LoadResponseBody("verification-verify.json");
        _handler.EnqueueJson(200, body);

        var resource = new VerificationResource(_client);
        VerificationResponse? result = await resource.VerifyAsync("ev-uuid-001");

        Assert.NotNull(result);
        Assert.Equal("ev-uuid-001", result!.EvidenceId);
        Assert.Equal("COMPLETED", result.Status);
    }

    [Fact]
    public async Task Verification_Verify_NoAuthHeaderSent()
    {
        string body = FixtureLoader.LoadResponseBody("verification-verify.json");
        _handler.EnqueueJson(200, body);

        var resource = new VerificationResource(_client);
        await resource.VerifyAsync("ev-uuid-001");

        Assert.Single(_handler.Requests);
        Assert.False(_handler.Requests[0].Headers.Contains("Authorization"));
    }

    [Fact]
    public async Task Verification_Verify_HasSignerInfo()
    {
        string body = FixtureLoader.LoadResponseBody("verification-verify.json");
        _handler.EnqueueJson(200, body);

        var resource = new VerificationResource(_client);
        VerificationResponse? result = await resource.VerifyAsync("ev-uuid-001");

        Assert.NotNull(result!.Signer);
        Assert.Equal("Jo\u00e3o Silva", result.Signer!.DisplayName);
    }

    [Fact]
    public async Task Verification_Verify_HasSteps()
    {
        string body = FixtureLoader.LoadResponseBody("verification-verify.json");
        _handler.EnqueueJson(200, body);

        var resource = new VerificationResource(_client);
        VerificationResponse? result = await resource.VerifyAsync("ev-uuid-001");

        Assert.NotNull(result!.Steps);
        Assert.Single(result.Steps!);
        Assert.Equal("CLICK_ACCEPT", result.Steps![0].Type);
    }

    // --- Evidence ---

    [Fact]
    public async Task Evidence_Get_ReturnsEvidence()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("evidence-get.json");
        _handler.EnqueueJson(200, body);

        var resource = new EvidenceResource(_client);
        Evidence? result = await resource.GetAsync("tx-uuid-001");

        Assert.NotNull(result);
        Assert.Equal("ev-uuid-001", result!.EvidenceId);
        Assert.Equal("COMPLETED", result.Status);
    }

    [Fact]
    public async Task Evidence_Get_HasDocument()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("evidence-get.json");
        _handler.EnqueueJson(200, body);

        var resource = new EvidenceResource(_client);
        Evidence? result = await resource.GetAsync("tx-uuid-001");

        Assert.NotNull(result!.Document);
        Assert.Equal("contract.pdf", result.Document!.Filename);
    }
}
