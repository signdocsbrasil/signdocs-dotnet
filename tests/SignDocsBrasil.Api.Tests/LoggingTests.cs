using System.Text.Json;
using Microsoft.Extensions.Logging;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Tests.Helpers;

namespace SignDocsBrasil.Api.Tests;

public class LoggingTests : IDisposable
{
    private readonly TestLogger _logger = new();
    private readonly MockHttpHandler _handler;
    private readonly SignDocsHttpClient _client;

    public LoggingTests()
    {
        _handler = new MockHttpHandler();
        var authHttpClient = new HttpClient(_handler) { BaseAddress = new Uri("https://api.test.com") };

        var auth = new AuthHandler(
            clientId: "test-client-id",
            clientSecret: "test-secret",
            privateKeyPem: null,
            kid: null,
            tokenUrl: "https://api.test.com/oauth2/token",
            scopes: new[] { "transactions:read" },
            testHttpClient: authHttpClient);

        var apiHttpClient = new HttpClient(_handler) { BaseAddress = new Uri("https://api.test.com") };

        _client = new SignDocsHttpClient(
            apiHttpClient,
            "https://api.test.com",
            TimeSpan.FromSeconds(30),
            auth,
            0,
            _logger);
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task SuccessfulRequest_LogsAtInformationLevel()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Information && e.Message.Contains("200"));
    }

    [Fact]
    public async Task FailedRequest_LogsAtWarningLevel()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(400,
            """{"type":"test","title":"Bad Request","status":400,"detail":"Invalid"}""");

        try
        {
            await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");
        }
        catch { /* expected */ }

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains("400"));
    }

    [Fact]
    public async Task SuccessfulRequest_LogsMethod()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/transactions");

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Information && e.Message.Contains("GET"));
    }

    [Fact]
    public async Task SuccessfulRequest_LogsPath()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/transactions");

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Information && e.Message.Contains("/v1/transactions"));
    }

    [Fact]
    public async Task PostRequest_LogsMethod()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(201, """{"id":"new"}""");

        await _client.RequestAsync<JsonElement>(HttpMethod.Post, "/v1/items", body: new { });

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Information && e.Message.Contains("POST"));
    }

    [Fact]
    public async Task AuthHeaders_AreNotLogged()
    {
        _handler.EnqueueToken("super-secret-token-12345");
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");

        foreach (var entry in _logger.Entries)
        {
            Assert.DoesNotContain("super-secret-token-12345", entry.Message);
        }
    }

    [Fact]
    public async Task Error500_LogsWarning()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(500,
            """{"type":"test","title":"Error","status":500,"detail":"Boom"}""");

        try
        {
            await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");
        }
        catch { /* expected */ }

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Warning && e.Message.Contains("500"));
    }

    [Fact]
    public async Task Error404_LogsWarning()
    {
        _handler.EnqueueToken();
        _handler.EnqueueProblemJson(404,
            """{"type":"test","title":"Not Found","status":404,"detail":"Missing"}""");

        try
        {
            await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/test");
        }
        catch { /* expected */ }

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task NoAuthRequest_LogsSuccessfully()
    {
        _handler.EnqueueJson(200, """{"status":"healthy"}""");

        await _client.RequestNoAuthAsync<JsonElement>(HttpMethod.Get, "/health");

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Information && e.Message.Contains("200"));
    }

    [Fact]
    public async Task MultipleRequests_LogEachOne()
    {
        _handler.EnqueueToken();
        _handler.EnqueueJson(200, """{"ok":true}""");
        _handler.EnqueueJson(200, """{"ok":true}""");

        await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/first");
        await _client.RequestAsync<JsonElement>(HttpMethod.Get, "/v1/second");

        var infoEntries = _logger.Entries.Where(e => e.Level == LogLevel.Information).ToList();
        Assert.True(infoEntries.Count >= 2);
    }

    /// <summary>
    /// Simple in-memory logger for testing.
    /// </summary>
    private class TestLogger : ILogger
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
        }
    }

    private record LogEntry(LogLevel Level, string Message);
}
