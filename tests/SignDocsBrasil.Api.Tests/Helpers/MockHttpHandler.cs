using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace SignDocsBrasil.Api.Tests.Helpers;

internal class MockHttpHandler : HttpMessageHandler
{
    private readonly ConcurrentQueue<HttpResponseMessage> _queue = new();

    internal List<HttpRequestMessage> Requests { get; } = new();

    /// <summary>
    /// Eagerly captured request body strings, indexed by request order.
    /// Use this instead of reading request.Content directly (which may be disposed).
    /// </summary>
    internal List<string?> RequestBodies { get; } = new();

    internal void EnqueueJson(int statusCode, string json)
    {
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        _queue.Enqueue(response);
    }

    internal void EnqueueProblemJson(int statusCode, string json)
    {
        var response = new HttpResponseMessage((HttpStatusCode)statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/problem+json")
        };
        _queue.Enqueue(response);
    }

    internal void EnqueueToken(string token = "test-token", int expiresIn = 900)
    {
        string json = $$"""{"access_token":"{{token}}","token_type":"Bearer","expires_in":{{expiresIn}},"scope":"transactions:read transactions:write"}""";
        EnqueueJson(200, json);
    }

    internal void EnqueueNoContent()
    {
        _queue.Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
    }

    internal void Enqueue(HttpResponseMessage response)
    {
        _queue.Enqueue(response);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Eagerly read the body before it gets disposed
        string? bodyText = null;
        if (request.Content is not null)
        {
            bodyText = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        Requests.Add(request);
        RequestBodies.Add(bodyText);

        if (_queue.TryDequeue(out HttpResponseMessage? response))
        {
            return response;
        }

        return new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("""{"type":"mock-error","title":"No queued response","status":500,"detail":"MockHttpHandler queue was empty"}""",
                Encoding.UTF8, "application/problem+json")
        };
    }
}
