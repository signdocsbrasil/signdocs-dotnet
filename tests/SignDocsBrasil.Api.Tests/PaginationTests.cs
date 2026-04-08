using System.Text.Json;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;
using SignDocsBrasil.Api.Resources;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class PaginationTests : IDisposable
{
    private readonly SignDocsHttpClient _client;
    private readonly MockHttpHandler _handler;
    private readonly TransactionsResource _transactions;

    public PaginationTests()
    {
        (_client, _handler) = TestClientFactory.Create();
        _transactions = new TransactionsResource(_client);
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task EmptyFirstPage_YieldsNoResults()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"transactions":[],"count":0}""");

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync())
        {
            items.Add(tx);
        }

        Assert.Empty(items);
    }

    [Fact]
    public async Task NullTransactions_YieldsNoResults()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"count":0}""");

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync())
        {
            items.Add(tx);
        }

        Assert.Empty(items);
    }

    [Fact]
    public async Task SinglePage_YieldsAllItems()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """
        {
            "transactions": [
                {"transactionId":"tx-001","status":"COMPLETED"},
                {"transactionId":"tx-002","status":"COMPLETED"}
            ],
            "count": 2
        }
        """);

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync())
        {
            items.Add(tx);
        }

        Assert.Equal(2, items.Count);
        Assert.Equal("tx-001", items[0].TransactionId);
        Assert.Equal("tx-002", items[1].TransactionId);
    }

    [Fact]
    public async Task MultiPage_FetchesAllItems()
    {
        _handler.EnqueueToken();

        // Page 1 with nextToken
        _handler.EnqueueJson(200, """
        {
            "transactions": [
                {"transactionId":"tx-001","status":"COMPLETED"},
                {"transactionId":"tx-002","status":"COMPLETED"}
            ],
            "nextToken": "page2token",
            "count": 2
        }
        """);

        // Page 2 with nextToken
        _handler.EnqueueJson(200, """
        {
            "transactions": [
                {"transactionId":"tx-003","status":"COMPLETED"}
            ],
            "nextToken": "page3token",
            "count": 1
        }
        """);

        // Page 3 without nextToken (last page)
        _handler.EnqueueJson(200, """
        {
            "transactions": [
                {"transactionId":"tx-004","status":"COMPLETED"}
            ],
            "count": 1
        }
        """);

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync())
        {
            items.Add(tx);
        }

        Assert.Equal(4, items.Count);
        Assert.Equal("tx-001", items[0].TransactionId);
        Assert.Equal("tx-002", items[1].TransactionId);
        Assert.Equal("tx-003", items[2].TransactionId);
        Assert.Equal("tx-004", items[3].TransactionId);
    }

    [Fact]
    public async Task StopsWhenNextTokenIsNull()
    {
        _handler.EnqueueToken();

        // Page 1 with nextToken
        _handler.EnqueueJson(200, """
        {
            "transactions": [
                {"transactionId":"tx-001","status":"CREATED"}
            ],
            "nextToken": "tok",
            "count": 1
        }
        """);

        // Page 2 without nextToken
        _handler.EnqueueJson(200, """
        {
            "transactions": [
                {"transactionId":"tx-002","status":"CREATED"}
            ],
            "nextToken": null,
            "count": 1
        }
        """);

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync())
        {
            items.Add(tx);
        }

        Assert.Equal(2, items.Count);
        // 1 token request + 2 list requests = 3 total
        Assert.Equal(3, _handler.Requests.Count);
    }

    [Fact]
    public async Task StopsWhenNextTokenIsEmpty()
    {
        _handler.EnqueueToken();

        _handler.EnqueueJson(200, """
        {
            "transactions": [
                {"transactionId":"tx-001","status":"CREATED"}
            ],
            "nextToken": "",
            "count": 1
        }
        """);

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync())
        {
            items.Add(tx);
        }

        Assert.Single(items);
    }

    [Fact]
    public async Task PassesNextTokenToSubsequentRequests()
    {
        _handler.EnqueueToken();

        _handler.EnqueueJson(200, """
        {
            "transactions": [{"transactionId":"tx-001","status":"OK"}],
            "nextToken": "page2-token-xyz",
            "count": 1
        }
        """);

        _handler.EnqueueJson(200, """
        {
            "transactions": [{"transactionId":"tx-002","status":"OK"}],
            "count": 1
        }
        """);

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync())
        {
            items.Add(tx);
        }

        // Second list request (index 2) should include nextToken
        HttpRequestMessage secondListReq = _handler.Requests[2];
        string url = secondListReq.RequestUri!.ToString();
        Assert.Contains("nextToken=page2-token-xyz", url);
    }

    [Fact]
    public async Task PassesOriginalQueryParams()
    {
        _handler.EnqueueToken();

        _handler.EnqueueJson(200, """
        {
            "transactions": [{"transactionId":"tx-001","status":"COMPLETED"}],
            "count": 1
        }
        """);

        var listParams = new TransactionListParams { Status = "COMPLETED", Limit = 5 };

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync(listParams))
        {
            items.Add(tx);
        }

        HttpRequestMessage apiReq = _handler.Requests[1];
        string url = apiReq.RequestUri!.ToString();
        Assert.Contains("status=COMPLETED", url);
        Assert.Contains("limit=5", url);
    }

    [Fact]
    public async Task RespectsCancellationToken()
    {
        _handler.EnqueueToken();

        _handler.EnqueueJson(200, """
        {
            "transactions": [{"transactionId":"tx-001","status":"OK"}],
            "nextToken": "more",
            "count": 1
        }
        """);

        // Enqueue a second page so it would keep going if not cancelled
        _handler.EnqueueJson(200, """
        {
            "transactions": [{"transactionId":"tx-002","status":"OK"}],
            "count": 1
        }
        """);

        using var cts = new CancellationTokenSource();

        var items = new List<Transaction>();
        try
        {
            await foreach (var tx in _transactions.ListAutoPaginateAsync(ct: cts.Token))
            {
                items.Add(tx);
                cts.Cancel(); // Cancel after first item
            }
        }
        catch (OperationCanceledException)
        {
            // Expected: cancellation is thrown on next MoveNext
        }

        // Should have gotten only the first item
        Assert.Single(items);
    }

    [Fact]
    public async Task DoesNotMutateOriginalParams()
    {
        _handler.EnqueueToken();

        _handler.EnqueueJson(200, """
        {
            "transactions": [{"transactionId":"tx-001","status":"OK"}],
            "nextToken": "page2",
            "count": 1
        }
        """);
        _handler.EnqueueJson(200, """
        {
            "transactions": [{"transactionId":"tx-002","status":"OK"}],
            "count": 1
        }
        """);

        var originalParams = new TransactionListParams { Status = "COMPLETED" };

        var items = new List<Transaction>();
        await foreach (var tx in _transactions.ListAutoPaginateAsync(originalParams))
        {
            items.Add(tx);
        }

        Assert.Null(originalParams.NextToken);
    }
}
