using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using KoChain.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace KoChain.Infrastructure.Services.Blockstream;

/// <summary>
/// Retrieves address data from the Blockstream Esplora API.
/// <para>
/// A standard Bitcoin node does not maintain an address index — it can look up
/// transactions by txid or block hash, but cannot answer "which transactions involve
/// address X?" without scanning every block. Blockstream runs its own indexed node
/// and exposes this data through the Esplora API, making it the right data source
/// for any address-centric query.
/// </para>
/// </summary>
public class BlockstreamAddressService : IAddressService
{
    private readonly HttpClient _httpClient;
    private readonly BlockstreamSettings _settings;

    /// <param name="httpClient">
    /// HttpClient injected via the typed client pattern (<c>AddHttpClient&lt;IAddressService, ...&gt;</c>).
    /// </param>
    /// <param name="settings">Blockstream API configuration (base URL, network).</param>
    public BlockstreamAddressService(HttpClient httpClient, IOptions<BlockstreamSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    /// <summary>
    /// Returns a complete snapshot of a Bitcoin address: balance, UTXO set, and first page of transactions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Makes three sequential HTTP requests to Blockstream:
    /// <list type="number">
    ///   <item><c>GET /address/:address</c> — summary stats (balance, tx count).</item>
    ///   <item><c>GET /address/:address/utxo</c> — all unspent outputs currently held by the address.</item>
    ///   <item><c>GET /address/:address/txs</c> — most recent 25 transactions.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Balance calculation:</b> Balance is derived as <c>funded_txo_sum - spent_txo_sum</c>
    /// from the <c>chain_stats</c> object, both values in satoshis.
    /// </para>
    /// <para>
    /// For full paginated transaction history use <see cref="GetAddressTransactionsAsync"/>.
    /// </para>
    /// </remarks>
    /// <param name="address">The Bitcoin address (supports legacy, P2SH, and bech32 formats).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<AddressModel> GetAddressDataAsync(string address, CancellationToken ct = default)
    {
        var addressData = await _httpClient.GetFromJsonAsync<BlockstreamAddressResponse>(
            $"{_settings.BaseUrl}/address/{address}", ct);

        if (addressData == null)
            throw new InvalidOperationException($"No data returned for address {address}");

        var utxos = await _httpClient.GetFromJsonAsync<List<BlockstreamUtxoResponse>>(
            $"{_settings.BaseUrl}/address/{address}/utxo", ct) ?? [];

        var txs = await _httpClient.GetFromJsonAsync<List<BlockstreamTxResponse>>(
            $"{_settings.BaseUrl}/address/{address}/txs", ct) ?? [];

        return new AddressModel
        {
            Address = address,
            // Balance = total received - total spent, both in satoshis from chain_stats.
            BalanceSatoshi = addressData.chain_stats.funded_txo_sum - addressData.chain_stats.spent_txo_sum,
            TransactionCount = addressData.chain_stats.tx_count,
            Utxos = utxos.Select(u => new AddressUtxo
            {
                TxId = u.txid,
                Vout = u.vout,
                ValueSatoshi = u.value,
                ScriptPubKey = u.scriptpubkey
            }).ToList(),
            Transactions = txs.Select(BlockstreamMapper.ToAddressTransaction).ToList()
        };
    }

    /// <summary>
    /// Returns a page of transactions for an address, supporting cursor-based pagination.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Blockstream uses cursor-based pagination rather than page numbers. Pass the txid of
    /// the last transaction from the previous response as <paramref name="afterTxId"/> to
    /// retrieve the next page. Omit it to get the most recent transactions.
    /// </para>
    /// <para>
    /// Each page returns up to 25 transactions ordered from newest to oldest.
    /// </para>
    /// </remarks>
    /// <param name="address">The Bitcoin address to query.</param>
    /// <param name="afterTxId">
    /// The txid of the last transaction from the previous page. Pass <c>null</c> for the first page.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Up to 25 transactions, ordered newest to oldest.</returns>
    public async Task<List<AddressTransaction>> GetAddressTransactionsAsync(string address, string? afterTxId = null, CancellationToken ct = default)
    {
        // Without afterTxId: returns the most recent 25 transactions.
        // With afterTxId: returns the next 25 transactions after that cursor.
        var url = afterTxId is null
            ? $"{_settings.BaseUrl}/address/{address}/txs"
            : $"{_settings.BaseUrl}/address/{address}/txs/chain/{afterTxId}";

        var txs = await _httpClient.GetFromJsonAsync<List<BlockstreamTxResponse>>(url, ct) ?? [];
        return txs.Select(BlockstreamMapper.ToAddressTransaction).ToList();
    }
}
