using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using KoChain.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace KoChain.Infrastructure.Services.Blockstream;

/// <summary>
/// Retrieves mempool data from the Blockstream Esplora API.
/// <para>
/// The mempool is the pool of unconfirmed transactions waiting to be included in a block.
/// Blockstream indexes the mempool in real time and exposes summary stats and recent entries
/// via their Esplora API.
/// </para>
/// </summary>
public class BlockstreamMempoolService : IMempoolService
{
    private readonly HttpClient _httpClient;
    private readonly BlockstreamSettings _settings;

    public BlockstreamMempoolService(HttpClient httpClient, IOptions<BlockstreamSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    /// <summary>
    /// Returns aggregate statistics about the current mempool state.
    /// </summary>
    /// <remarks>
    /// Useful for displaying network congestion — high transaction count and virtual size
    /// indicate a congested mempool with elevated fees.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Transaction count, total virtual size in vbytes, and total fees in satoshis.</returns>
    public async Task<MempoolStats> GetMempoolStatsAsync(CancellationToken ct = default)
    {
        var stats = await _httpClient.GetFromJsonAsync<BlockstreamMempoolStats>(
            $"{_settings.BaseUrl}/mempool", ct);

        if (stats == null)
            throw new InvalidOperationException("Failed to retrieve mempool stats.");

        return new MempoolStats
        {
            TransactionCount = stats.count,
            VirtualSizeBytes = stats.vsize,
            TotalFeeSatoshi = stats.total_fee
        };
    }

    /// <summary>
    /// Returns the most recently added unconfirmed transactions (up to 10).
    /// </summary>
    /// <remarks>
    /// These are the transactions that entered the mempool most recently. Useful for
    /// showing live network activity on an explorer homepage.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Up to 10 of the most recently submitted unconfirmed transactions.</returns>
    public async Task<List<TransactionModel>> GetRecentTransactionsAsync(CancellationToken ct = default)
    {
        var txs = await _httpClient.GetFromJsonAsync<List<BlockstreamTxResponse>>(
            $"{_settings.BaseUrl}/mempool/recent", ct) ?? [];

        return txs.Select(BlockstreamMapper.ToTransactionModel).ToList();
    }
}
