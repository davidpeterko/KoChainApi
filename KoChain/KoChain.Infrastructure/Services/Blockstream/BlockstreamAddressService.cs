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
    /// Returns a complete snapshot of a Bitcoin address: balance, UTXO set, and transaction history.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Makes three sequential HTTP requests to Blockstream:
    /// <list type="number">
    ///   <item><c>GET /address/:address</c> — summary stats (balance, tx count).</item>
    ///   <item><c>GET /address/:address/utxo</c> — all unspent outputs currently held by the address.</item>
    ///   <item><c>GET /address/:address/txs</c> — transaction history (most recent 25 by default).</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Balance calculation:</b> The Blockstream API does not return a single balance field.
    /// Balance is derived as <c>funded_txo_sum - spent_txo_sum</c> from the <c>chain_stats</c>
    /// object, both values in satoshis.
    /// </para>
    /// <para>
    /// <b>Confirmations:</b> Blockstream does not return an exact confirmation count per transaction.
    /// <c>Confirmations</c> is set to 0 for unconfirmed and 1 for confirmed. To compute an exact
    /// count, compare the transaction's <c>BlockHeight</c> against the current chain tip height.
    /// </para>
    /// <para>
    /// <b>Pagination:</b> The <c>/txs</c> endpoint returns a maximum of 25 transactions per call.
    /// Pagination support (via <c>/txs/chain/:last_seen_txid</c>) is not yet implemented.
    /// </para>
    /// </remarks>
    /// <param name="address">The Bitcoin address (supports legacy, P2SH, and bech32 formats).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// An <see cref="AddressModel"/> with balance, UTXOs, and recent transaction history.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the API returns no data for the given address.</exception>
    /// <exception cref="HttpRequestException">Thrown if the Blockstream API is unreachable.</exception>
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
            Transactions = txs.Select(tx => new AddressTransaction
            {
                TxId = tx.txid,
                FeeSatoshi = tx.fee,
                Confirmations = tx.status.confirmed ? 1 : 0,
                Timestamp = tx.status.block_time.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(tx.status.block_time.Value)
                    : null,
                Inputs = tx.vin.Select(i => new TransactionInput
                {
                    PrevTxId = i.txid,
                    OutputIndex = i.vout,
                    // prevout is null for coinbase transactions (block reward inputs).
                    Address = i.prevout?.scriptpubkey_address ?? string.Empty,
                    ValueSatoshi = i.prevout?.value ?? 0
                }).ToList(),
                Outputs = tx.vout.Select((o, index) => new TransactionOutput
                {
                    // scriptpubkey_address is null for unspendable outputs (e.g. OP_RETURN).
                    Address = o.scriptpubkey_address ?? string.Empty,
                    ValueSatoshi = o.value,
                    Index = index
                }).ToList()
            }).ToList()
        };
    }
}
