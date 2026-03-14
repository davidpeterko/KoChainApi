using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using KoChain.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace KoChain.Infrastructure.Services.Blockstream;

/// <summary>
/// Retrieves transaction data from the Blockstream Esplora API.
/// <para>
/// Preferred over direct RPC for transaction lookups. A bare Bitcoin node requires N+1 RPC
/// calls to resolve inputs — one call per input to look up the previous output for its address
/// and amount. Blockstream's API returns fully resolved <c>prevout</c> data on every input
/// in a single HTTP response.
/// </para>
/// </summary>
public class BlockstreamTransactionService : ITransactionService
{
    private readonly HttpClient _httpClient;
    private readonly BlockstreamSettings _settings;

    /// <param name="httpClient">
    /// HttpClient injected via the typed client pattern (<c>AddHttpClient&lt;ITransactionService, ...&gt;</c>).
    /// </param>
    /// <param name="settings">Blockstream API configuration (base URL, network).</param>
    public BlockstreamTransactionService(HttpClient httpClient, IOptions<BlockstreamSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    /// <summary>
    /// Returns full details for a transaction, including all inputs with their source
    /// addresses and amounts, and all outputs with their destination addresses and amounts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bitcoin uses a UTXO (Unspent Transaction Output) model. Each transaction input
    /// references a specific output from a previous transaction. Knowing the sending address
    /// and amount of an input therefore requires looking up that previous transaction's output.
    /// Blockstream resolves this server-side and includes <c>prevout</c> data on each input.
    /// </para>
    /// <para>
    /// <b>Confirmations:</b> Blockstream does not return a confirmation count directly.
    /// <c>Confirmations</c> is set to 0 for unconfirmed transactions and 1 for confirmed ones.
    /// An exact count can be derived by comparing <c>BlockHeight</c> against the current
    /// chain tip, which requires a separate call to <c>GET /api/blocks/tip</c>.
    /// </para>
    /// </remarks>
    /// <param name="txId">The transaction ID (txid) as a 64-character hex string.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="TransactionModel"/> with fully populated inputs, outputs, fee, and status.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the API returns no data for the given txid.</exception>
    /// <exception cref="HttpRequestException">Thrown if the Blockstream API is unreachable.</exception>
    public async Task<TransactionModel> GetTransactionAsync(string txId, CancellationToken ct = default)
    {
        var tx = await _httpClient.GetFromJsonAsync<BlockstreamTxResponse>(
            $"{_settings.BaseUrl}/tx/{txId}", ct);

        if (tx == null)
            throw new InvalidOperationException($"No transaction found for txId {txId}");

        return BlockstreamMapper.ToTransactionModel(tx);
    }

    /// <summary>
    /// Returns a page of transactions contained in the given block.
    /// </summary>
    /// <remarks>
    /// Blockstream's <c>GET /block/:hash/txs/:start_index</c> endpoint returns up to 25
    /// transactions per call. <paramref name="page"/> is converted to a start index
    /// internally (<c>page * 25</c>), so page 0 = transactions 0–24, page 1 = 25–49, etc.
    /// </remarks>
    /// <param name="blockHash">The block hash as a 64-character hex string.</param>
    /// <param name="page">Zero-based page number.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Up to 25 transactions from the specified block page.</returns>
    /// <exception cref="HttpRequestException">Thrown if the block hash is not found or the API is unreachable.</exception>
    public async Task<List<TransactionModel>> GetBlockTransactionsAsync(string blockHash, int page = 0, CancellationToken ct = default)
    {
        var startIndex = page * 25;
        var txs = await _httpClient.GetFromJsonAsync<List<BlockstreamTxResponse>>(
            $"{_settings.BaseUrl}/block/{blockHash}/txs/{startIndex}", ct) ?? [];

        return txs.Select(BlockstreamMapper.ToTransactionModel).ToList();
    }
}
