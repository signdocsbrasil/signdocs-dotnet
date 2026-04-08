using System.Text.Json;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class ModelsTests
{
    // --- Transaction ---

    [Fact]
    public void Transaction_DeserializesFromFixture()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-create.json");
        var tx = JsonSerializer.Deserialize<Transaction>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(tx);
        Assert.Equal("abc123", tx!.TenantId);
        Assert.Equal("tx-uuid-001", tx.TransactionId);
        Assert.Equal("CREATED", tx.Status);
        Assert.Equal("DOCUMENT_SIGNATURE", tx.Purpose);
    }

    [Fact]
    public void Transaction_HasPolicy()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-create.json");
        var tx = JsonSerializer.Deserialize<Transaction>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(tx!.Policy);
        Assert.Equal("CLICK_ONLY", tx.Policy!.Profile);
    }

    [Fact]
    public void Transaction_HasSigner()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-create.json");
        var tx = JsonSerializer.Deserialize<Transaction>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(tx!.Signer);
        Assert.Equal("joao@example.com", tx.Signer!.Email);
        Assert.Equal("12345678901", tx.Signer.Cpf);
    }

    [Fact]
    public void Transaction_HasSteps()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-create.json");
        var tx = JsonSerializer.Deserialize<Transaction>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(tx!.Steps);
        Assert.Single(tx.Steps!);
        Assert.Equal("step-uuid-001", tx.Steps![0].StepId);
        Assert.Equal("CLICK_ACCEPT", tx.Steps[0].Type);
        Assert.Equal("PENDING", tx.Steps[0].Status);
        Assert.Equal(1, tx.Steps[0].Order);
        Assert.Equal(0, tx.Steps[0].Attempts);
        Assert.Equal(3, tx.Steps[0].MaxAttempts);
    }

    [Fact]
    public void Transaction_HasMetadata()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-create.json");
        var tx = JsonSerializer.Deserialize<Transaction>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(tx!.Metadata);
        Assert.Equal("CTR-2024-001", tx.Metadata!["contractId"]);
    }

    [Fact]
    public void Transaction_HasTimestamps()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-create.json");
        var tx = JsonSerializer.Deserialize<Transaction>(json, SignDocsHttpClient.JsonOptions);

        Assert.Equal("2024-11-16T00:00:00.000Z", tx!.ExpiresAt);
        Assert.Equal("2024-11-15T00:00:00.000Z", tx.CreatedAt);
        Assert.Equal("2024-11-15T00:00:00.000Z", tx.UpdatedAt);
    }

    [Fact]
    public void Transaction_GetFixture_HasMultipleSteps()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-get.json");
        var tx = JsonSerializer.Deserialize<Transaction>(json, SignDocsHttpClient.JsonOptions);

        Assert.Equal(2, tx!.Steps!.Count);
        Assert.Equal("COMPLETED", tx.Steps[0].Status);
        Assert.Equal("PENDING", tx.Steps[1].Status);
    }

    // --- TransactionListResponse ---

    [Fact]
    public void TransactionListResponse_DeserializesFromFixture()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-list.json");
        var list = JsonSerializer.Deserialize<TransactionListResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(list);
        Assert.Equal(2, list!.Count);
        Assert.NotNull(list.Transactions);
        Assert.Equal(2, list.Transactions!.Count);
    }

    [Fact]
    public void TransactionListResponse_HasNextToken()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-list.json");
        var list = JsonSerializer.Deserialize<TransactionListResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(list!.NextToken);
        Assert.NotEmpty(list.NextToken!);
    }

    [Fact]
    public void TransactionListResponse_TransactionsHaveCorrectData()
    {
        string json = FixtureLoader.LoadResponseBody("transactions-list.json");
        var list = JsonSerializer.Deserialize<TransactionListResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.Equal("tx-uuid-002", list!.Transactions![0].TransactionId);
        Assert.Equal("COMPLETED", list.Transactions[0].Status);
        Assert.Equal("tx-uuid-003", list.Transactions[1].TransactionId);
    }

    // --- Evidence ---

    [Fact]
    public void Evidence_DeserializesFromFixture()
    {
        string json = FixtureLoader.LoadResponseBody("evidence-get.json");
        var ev = JsonSerializer.Deserialize<Evidence>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(ev);
        Assert.Equal("abc123", ev!.TenantId);
        Assert.Equal("tx-uuid-001", ev.TransactionId);
        Assert.Equal("ev-uuid-001", ev.EvidenceId);
        Assert.Equal("COMPLETED", ev.Status);
    }

    [Fact]
    public void Evidence_HasSigner()
    {
        string json = FixtureLoader.LoadResponseBody("evidence-get.json");
        var ev = JsonSerializer.Deserialize<Evidence>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(ev!.Signer);
        Assert.Equal("12345678901", ev.Signer!.Cpf);
        Assert.Equal("user-ext-001", ev.Signer.UserExternalId);
    }

    [Fact]
    public void Evidence_HasSteps()
    {
        string json = FixtureLoader.LoadResponseBody("evidence-get.json");
        var ev = JsonSerializer.Deserialize<Evidence>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(ev!.Steps);
        Assert.Single(ev.Steps!);
        Assert.Equal("CLICK_ACCEPT", ev.Steps![0].Type);
        Assert.Equal("COMPLETED", ev.Steps[0].Status);
    }

    [Fact]
    public void Evidence_HasDocument()
    {
        string json = FixtureLoader.LoadResponseBody("evidence-get.json");
        var ev = JsonSerializer.Deserialize<Evidence>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(ev!.Document);
        Assert.Equal("contract.pdf", ev.Document!.Filename);
        Assert.NotNull(ev.Document.Hash);
    }

    [Fact]
    public void Evidence_HasTimestamps()
    {
        string json = FixtureLoader.LoadResponseBody("evidence-get.json");
        var ev = JsonSerializer.Deserialize<Evidence>(json, SignDocsHttpClient.JsonOptions);

        Assert.Equal("2024-11-15T00:00:00.000Z", ev!.CreatedAt);
        Assert.Equal("2024-11-15T00:01:00.000Z", ev.CompletedAt);
    }

    // --- HealthCheckResponse ---

    [Fact]
    public void HealthCheckResponse_DeserializesFromFixture()
    {
        string json = FixtureLoader.LoadResponseBody("health-check.json");
        var health = JsonSerializer.Deserialize<HealthCheckResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(health);
        Assert.Equal("healthy", health!.Status);
        Assert.Equal("1.0.0", health.Version);
        Assert.Equal("2024-11-15T12:00:00.000Z", health.Timestamp);
    }

    [Fact]
    public void HealthCheckResponse_HasServices()
    {
        string json = FixtureLoader.LoadResponseBody("health-check.json");
        var health = JsonSerializer.Deserialize<HealthCheckResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(health!.Services);
        Assert.Equal(3, health.Services!.Count);
    }

    [Fact]
    public void HealthCheckResponse_ServiceStatusHasLatency()
    {
        string json = FixtureLoader.LoadResponseBody("health-check.json");
        var health = JsonSerializer.Deserialize<HealthCheckResponse>(json, SignDocsHttpClient.JsonOptions);

        ServiceStatus dynamo = health!.Services!["dynamodb"];
        Assert.Equal("healthy", dynamo.Status);
        Assert.Equal(12, dynamo.Latency);
    }

    [Fact]
    public void HealthCheckResponse_AllServicesHealthy()
    {
        string json = FixtureLoader.LoadResponseBody("health-check.json");
        var health = JsonSerializer.Deserialize<HealthCheckResponse>(json, SignDocsHttpClient.JsonOptions);

        foreach (var (_, svc) in health!.Services!)
        {
            Assert.Equal("healthy", svc.Status);
        }
    }

    // --- VerificationResponse ---

    [Fact]
    public void VerificationResponse_DeserializesFromFixture()
    {
        string json = FixtureLoader.LoadResponseBody("verification-verify.json");
        var v = JsonSerializer.Deserialize<VerificationResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(v);
        Assert.Equal("ev-uuid-001", v!.EvidenceId);
        Assert.Equal("COMPLETED", v.Status);
        Assert.Equal("Acme Corp", v.TenantName);
    }

    [Fact]
    public void VerificationResponse_HasPolicy()
    {
        string json = FixtureLoader.LoadResponseBody("verification-verify.json");
        var v = JsonSerializer.Deserialize<VerificationResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(v!.Policy);
        Assert.Equal("CLICK_ONLY", v.Policy!.Profile);
    }

    // --- RegisterWebhookResponse ---

    [Fact]
    public void RegisterWebhookResponse_DeserializesFromFixture()
    {
        string json = FixtureLoader.LoadResponseBody("webhooks-register.json");
        var wh = JsonSerializer.Deserialize<RegisterWebhookResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(wh);
        Assert.Equal("wh-uuid-001", wh!.WebhookId);
        Assert.Equal("https://example.com/webhooks/signdocs", wh.Url);
        Assert.Equal("ACTIVE", wh.Status);
        Assert.NotNull(wh.Secret);
    }

    [Fact]
    public void RegisterWebhookResponse_HasEvents()
    {
        string json = FixtureLoader.LoadResponseBody("webhooks-register.json");
        var wh = JsonSerializer.Deserialize<RegisterWebhookResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(wh!.Events);
        Assert.Equal(2, wh.Events!.Count);
        Assert.Contains("TRANSACTION.COMPLETED", wh.Events);
        Assert.Contains("TRANSACTION.FAILED", wh.Events);
    }

    // --- EnrollUserResponse ---

    [Fact]
    public void EnrollUserResponse_DeserializesFromFixture()
    {
        string json = FixtureLoader.LoadResponseBody("users-enroll.json");
        var resp = JsonSerializer.Deserialize<EnrollUserResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(resp);
        Assert.Equal("user-ext-001", resp!.UserExternalId);
    }

    // --- CancelTransactionResponse ---

    [Fact]
    public void CancelTransactionResponse_Deserializes()
    {
        string json = """{"transactionId":"tx-001","status":"CANCELLED","cancelledAt":"2024-11-15T00:05:00.000Z"}""";
        var resp = JsonSerializer.Deserialize<CancelTransactionResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(resp);
        Assert.Equal("tx-001", resp!.TransactionId);
        Assert.Equal("CANCELLED", resp.Status);
    }

    // --- FinalizeResponse ---

    [Fact]
    public void FinalizeResponse_Deserializes()
    {
        string json = """{"transactionId":"tx-001","status":"COMPLETED","evidenceId":"ev-001","evidenceHash":"abc","completedAt":"2024-11-15T01:00:00.000Z"}""";
        var resp = JsonSerializer.Deserialize<FinalizeResponse>(json, SignDocsHttpClient.JsonOptions);

        Assert.NotNull(resp);
        Assert.Equal("COMPLETED", resp!.Status);
        Assert.Equal("ev-001", resp.EvidenceId);
    }

    // --- CreateTransactionRequest Serialization ---

    [Fact]
    public void CreateTransactionRequest_SerializesCorrectly()
    {
        var request = new CreateTransactionRequest
        {
            Purpose = "DOCUMENT_SIGNATURE",
            Policy = new Policy { Profile = "CLICK_ONLY" },
            Signer = new Signer { Name = "Test", Email = "test@example.com" },
            Metadata = new Dictionary<string, string> { { "key", "value" } }
        };

        string json = JsonSerializer.Serialize(request, SignDocsHttpClient.JsonOptions);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("DOCUMENT_SIGNATURE", doc.RootElement.GetProperty("purpose").GetString());
        Assert.Equal("CLICK_ONLY", doc.RootElement.GetProperty("policy").GetProperty("profile").GetString());
    }

    [Fact]
    public void CreateTransactionRequest_OmitsNullFields()
    {
        var request = new CreateTransactionRequest
        {
            Purpose = "DOCUMENT_SIGNATURE"
        };

        string json = JsonSerializer.Serialize(request, SignDocsHttpClient.JsonOptions);
        using var doc = JsonDocument.Parse(json);

        Assert.False(doc.RootElement.TryGetProperty("signer", out _));
        Assert.False(doc.RootElement.TryGetProperty("metadata", out _));
        Assert.False(doc.RootElement.TryGetProperty("documentGroupId", out _));
    }
}
