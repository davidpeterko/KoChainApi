using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using KoChain.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace KoChain.Infrastructure.Services.Blockstream;

/// <summary>
/// Retrieves full transaction details from the Blockstream Esplora API.
/// <para>
/// This is preferred over direct RPC for transaction lookups. A bare Bitcoin node requires
/// N+1 RPC calls to resolve transaction inputs — one call to fetch the transaction itself,
/// then one additional call per input to look up the previous output (in order to determine
/// the sending address and amount). Blockstream's <c>GET /tx/:txid</c> endpoint returns
/// all of this in a single HTTP response, including fully resolved <c>prevout</c> data
/// on every input.
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
    /// chain tip, which requires a separate call.
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
        var url = $"{_settings.BaseUrl}/tx/{txId}";
        var tx = await _httpClient.GetFromJsonAsync<BlockstreamTxResponse>(url, ct);

        if (tx == null)
            throw new InvalidOperationException($"No transaction found for txId {txId}");

        return new TransactionModel
        {
            TxId = txId,
            FeeSatoshi = tx.fee,
            // Blockstream does not return a confirmation count — only a confirmed flag.
            // 0 = unconfirmed (in mempool), 1 = confirmed in a block.
            Confirmations = tx.status.confirmed ? 1 : 0,
            BlockHeight = tx.status.block_height,
            Timestamp = tx.status.block_time.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(tx.status.block_time.Value)
                : null,
            Inputs = tx.vin.Select(i => new TransactionInput
            {
                PrevTxId = i.txid,
                OutputIndex = i.vout,
                // prevout is null for coinbase transactions (block reward), which have no previous output.
                Address = i.prevout?.scriptpubkey_address ?? string.Empty,
                ValueSatoshi = i.prevout?.value ?? 0
            }).ToList(),
            Outputs = tx.vout.Select((o, index) => new TransactionOutput
            {
                // scriptpubkey_address is null for unspendable outputs (e.g. OP_RETURN data outputs).
                Address = o.scriptpubkey_address ?? string.Empty,
                ValueSatoshi = o.value,
                Index = index
            }).ToList()
        };
    }
}
