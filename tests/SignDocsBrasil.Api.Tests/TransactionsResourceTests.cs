using System.Text.Json;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;
using SignDocsBrasil.Api.Resources;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class TransactionsResourceTests : IDisposable
{
    private readonly SignDocsHttpClient _client;
    private readonly MockHttpHandler _handler;
    private readonly TransactionsResource _transactions;

    public TransactionsResourceTests()
    {
        (_client, _handler) = TestClientFactory.Create();
        _transactions = new TransactionsResource(_client);
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task Create_ReturnsTransaction()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-create.json");
        _handler.EnqueueJson(201, body);

        var request = new CreateTransactionRequest
        {
            Purpose = "DOCUMENT_SIGNATURE",
            Policy = new Policy { Profile = "CLICK_ONLY" },
            Signer = new Signer
            {
                Name = "Joao Silva",
                Email = "joao@example.com",
                UserExternalId = "user-ext-001",
                Cpf = "12345678901"
            }
        };

        Transaction? result = await _transactions.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("tx-uuid-001", result!.TransactionId);
        Assert.Equal("CREATED", result.Status);
        Assert.Equal("DOCUMENT_SIGNATURE", result.Purpose);
    }

    [Fact]
    public async Task Create_IncludesSigner()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-create.json");
        _handler.EnqueueJson(201, body);

        var request = new CreateTransactionRequest();
        Transaction? result = await _transactions.CreateAsync(request);

        Assert.NotNull(result!.Signer);
        Assert.Equal("joao@example.com", result.Signer!.Email);
    }

    [Fact]
    public async Task Create_IncludesSteps()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-create.json");
        _handler.EnqueueJson(201, body);

        var request = new CreateTransactionRequest();
        Transaction? result = await _transactions.CreateAsync(request);

        Assert.NotNull(result!.Steps);
        Assert.Single(result.Steps!);
        Assert.Equal("CLICK_ACCEPT", result.Steps![0].Type);
        Assert.Equal("PENDING", result.Steps[0].Status);
    }

    [Fact]
    public async Task Create_IncludesPolicy()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-create.json");
        _handler.EnqueueJson(201, body);

        var request = new CreateTransactionRequest();
        Transaction? result = await _transactions.CreateAsync(request);

        Assert.NotNull(result!.Policy);
        Assert.Equal("CLICK_ONLY", result.Policy!.Profile);
    }

    [Fact]
    public async Task Create_IncludesMetadata()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-create.json");
        _handler.EnqueueJson(201, body);

        var request = new CreateTransactionRequest();
        Transaction? result = await _transactions.CreateAsync(request);

        Assert.NotNull(result!.Metadata);
        Assert.Equal("CTR-2024-001", result.Metadata!["contractId"]);
    }

    [Fact]
    public async Task Create_SetsIdempotencyKey()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-create.json");
        _handler.EnqueueJson(201, body);

        var request = new CreateTransactionRequest();
        await _transactions.CreateAsync(request, idempotencyKey: "my-idem-key");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal("my-idem-key",
            apiRequest.Headers.GetValues("X-Idempotency-Key").First());
    }

    [Fact]
    public async Task Create_UsesPostMethod()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-create.json");
        _handler.EnqueueJson(201, body);

        var request = new CreateTransactionRequest();
        await _transactions.CreateAsync(request);

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal(HttpMethod.Post, apiRequest.Method);
        Assert.EndsWith("/v1/transactions", apiRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task List_ReturnsTransactionListResponse()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-list.json");
        _handler.EnqueueJson(200, body);

        TransactionListResponse? result = await _transactions.ListAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal(2, result.Transactions!.Count);
        Assert.NotNull(result.NextToken);
    }

    [Fact]
    public async Task List_WithQueryParams()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-list.json");
        _handler.EnqueueJson(200, body);

        var listParams = new TransactionListParams
        {
            Status = "COMPLETED",
            Limit = 2
        };

        await _transactions.ListAsync(listParams);

        HttpRequestMessage apiRequest = _handler.Requests[1];
        string url = apiRequest.RequestUri!.ToString();
        Assert.Contains("status=COMPLETED", url);
        Assert.Contains("limit=2", url);
    }

    [Fact]
    public async Task List_WithAllQueryParams()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"transactions":[],"count":0}""");

        var listParams = new TransactionListParams
        {
            Status = "CREATED",
            UserExternalId = "user-001",
            DocumentGroupId = "dg-001",
            Limit = 5,
            NextToken = "token123",
            StartDate = "2024-01-01",
            EndDate = "2024-12-31"
        };

        await _transactions.ListAsync(listParams);

        HttpRequestMessage apiRequest = _handler.Requests[1];
        string url = apiRequest.RequestUri!.ToString();
        Assert.Contains("status=CREATED", url);
        Assert.Contains("userExternalId=user-001", url);
        Assert.Contains("documentGroupId=dg-001", url);
        Assert.Contains("limit=5", url);
        Assert.Contains("nextToken=token123", url);
        Assert.Contains("startDate=2024-01-01", url);
        Assert.Contains("endDate=2024-12-31", url);
    }

    [Fact]
    public async Task List_WithNullParams_SendsNoQuery()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"transactions":[],"count":0}""");

        await _transactions.ListAsync();

        HttpRequestMessage apiRequest = _handler.Requests[1];
        string url = apiRequest.RequestUri!.ToString();
        Assert.DoesNotContain("?", url);
    }

    [Fact]
    public async Task Get_ReturnsTransaction()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-get.json");
        _handler.EnqueueJson(200, body);

        Transaction? result = await _transactions.GetAsync("tx-uuid-001");

        Assert.NotNull(result);
        Assert.Equal("tx-uuid-001", result!.TransactionId);
        Assert.Equal("IN_PROGRESS", result.Status);
    }

    [Fact]
    public async Task Get_UsesCorrectPath()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-get.json");
        _handler.EnqueueJson(200, body);

        await _transactions.GetAsync("tx-uuid-001");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.EndsWith("/v1/transactions/tx-uuid-001", apiRequest.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Get, apiRequest.Method);
    }

    [Fact]
    public async Task Get_HasMultipleSteps()
    {
        _handler.EnqueueToken();
        string body = FixtureLoader.LoadResponseBody("transactions-get.json");
        _handler.EnqueueJson(200, body);

        Transaction? result = await _transactions.GetAsync("tx-uuid-001");

        Assert.NotNull(result!.Steps);
        Assert.Equal(2, result.Steps!.Count);
        Assert.Equal("CLICK_ACCEPT", result.Steps[0].Type);
        Assert.Equal("OTP_CHALLENGE", result.Steps[1].Type);
    }

    [Fact]
    public async Task Cancel_ReturnsResponse()
    {
        _handler.EnqueueToken();
        // Cancel fixture returns a full Transaction, but CancelTransactionResponse is a subset
        _handler.EnqueueJson(200, """{"transactionId":"tx-uuid-001","status":"CANCELLED","cancelledAt":"2024-11-15T00:05:00.000Z"}""");

        CancelTransactionResponse? result = await _transactions.CancelAsync("tx-uuid-001");

        Assert.NotNull(result);
        Assert.Equal("tx-uuid-001", result!.TransactionId);
        Assert.Equal("CANCELLED", result.Status);
    }

    [Fact]
    public async Task Cancel_UsesDeleteMethod()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"transactionId":"tx-001","status":"CANCELLED"}""");

        await _transactions.CancelAsync("tx-001");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal(HttpMethod.Delete, apiRequest.Method);
        Assert.EndsWith("/v1/transactions/tx-001", apiRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Finalize_ReturnsResponse()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200,
            """{"transactionId":"tx-uuid-001","status":"COMPLETED","evidenceId":"ev-001","evidenceHash":"sha256-hash","completedAt":"2024-11-15T01:00:00.000Z"}""");

        FinalizeResponse? result = await _transactions.FinalizeAsync("tx-uuid-001");

        Assert.NotNull(result);
        Assert.Equal("tx-uuid-001", result!.TransactionId);
        Assert.Equal("COMPLETED", result.Status);
        Assert.Equal("ev-001", result.EvidenceId);
    }

    [Fact]
    public async Task Finalize_UsesPostMethod()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"transactionId":"tx-001","status":"COMPLETED"}""");

        await _transactions.FinalizeAsync("tx-001");

        HttpRequestMessage apiRequest = _handler.Requests[1];
        Assert.Equal(HttpMethod.Post, apiRequest.Method);
        Assert.EndsWith("/v1/transactions/tx-001/finalize", apiRequest.RequestUri!.ToString());
    }

    [Fact]
    public void TransactionListParams_Clone_CreatesDeepCopy()
    {
        var original = new TransactionListParams
        {
            Status = "CREATED",
            Limit = 10,
            NextToken = "token1"
        };

        TransactionListParams clone = original.Clone();
        clone.NextToken = "token2";

        Assert.Equal("token1", original.NextToken);
        Assert.Equal("token2", clone.NextToken);
    }

    [Fact]
    public void TransactionListParams_ToQueryDictionary_OmitsNulls()
    {
        var p = new TransactionListParams { Status = "CREATED" };
        Dictionary<string, string> dict = p.ToQueryDictionary();

        Assert.Single(dict);
        Assert.Equal("CREATED", dict["status"]);
    }

    [Fact]
    public void TransactionListParams_ToQueryDictionary_IncludesAllFields()
    {
        var p = new TransactionListParams
        {
            Status = "COMPLETED",
            UserExternalId = "u-1",
            DocumentGroupId = "dg-1",
            Limit = 5,
            NextToken = "tk",
            StartDate = "2024-01-01",
            EndDate = "2024-12-31"
        };

        Dictionary<string, string> dict = p.ToQueryDictionary();

        Assert.Equal(7, dict.Count);
    }
}
