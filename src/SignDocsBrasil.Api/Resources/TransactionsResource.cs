using System.Runtime.CompilerServices;
using SignDocsBrasil.Api.Internal;
using SignDocsBrasil.Api.Models;

namespace SignDocsBrasil.Api.Resources;

public sealed class TransactionsResource
{
    private readonly SignDocsHttpClient _client;

    internal TransactionsResource(SignDocsHttpClient client) => _client = client;

    public async Task<Transaction?> CreateAsync(
        CreateTransactionRequest request,
        string? idempotencyKey = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestWithIdempotencyAsync<Transaction>(
            HttpMethod.Post,
            "/v1/transactions",
            body: request,
            idempotencyKey: idempotencyKey,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<TransactionListResponse?> ListAsync(
        TransactionListParams? @params = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        Dictionary<string, string>? query = @params?.ToQueryDictionary();

        return await _client.RequestAsync<TransactionListResponse>(
            HttpMethod.Get,
            "/v1/transactions",
            query: query,
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<Transaction?> GetAsync(
        string transactionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<Transaction>(
            HttpMethod.Get,
            $"/v1/transactions/{transactionId}",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<CancelTransactionResponse?> CancelAsync(
        string transactionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<CancelTransactionResponse>(
            HttpMethod.Delete,
            $"/v1/transactions/{transactionId}",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async Task<FinalizeResponse?> FinalizeAsync(
        string transactionId,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        return await _client.RequestAsync<FinalizeResponse>(
            HttpMethod.Post,
            $"/v1/transactions/{transactionId}/finalize",
            timeout: timeout,
            ct: ct).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<Transaction> ListAutoPaginateAsync(
        TransactionListParams? @params = null,
        TimeSpan? timeout = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        TransactionListParams currentParams = @params?.Clone() ?? new TransactionListParams();

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            TransactionListResponse? page = await ListAsync(currentParams, timeout, ct)
                .ConfigureAwait(false);

            if (page?.Transactions is null or { Count: 0 })
                yield break;

            foreach (Transaction transaction in page.Transactions)
            {
                yield return transaction;
            }

            if (string.IsNullOrEmpty(page.NextToken))
                yield break;

            currentParams.NextToken = page.NextToken;
        }
    }
}
