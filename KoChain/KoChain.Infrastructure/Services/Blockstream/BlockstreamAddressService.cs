using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin.Address;
using KoChain.Core.Models.Blockstream;
using KoChain.Infrastructure.Configuration;
using System.Net.Http.Json;

namespace KoChain.Infrastructure.Services.Blockstream;

public class BlockstreamAddressService : IAddressService
{
    private readonly HttpClient _httpClient;
    private readonly BlockstreamSettings _settings;

    public BlockstreamAddressService(HttpClient httpClient, BlockstreamSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<AddressModel> GetAddressDataAsync(string address, CancellationToken cancellationToken = default)
    {
        // Fetch address summary (/address/:address)
        var url = $"{_settings.BaseUrl}/address/{address}";
        var addressData = await _httpClient.GetFromJsonAsync<BlockstreamAddressResponse>(url, cancellationToken);

        if (addressData == null)
            throw new InvalidOperationException($"No data returned for address {address}");

        // Fetch UTXOs (/address/:address/utxo)
        var utxoUrl = $"{_settings.BaseUrl}/address/{address}/utxo";
        var utxos = await _httpClient.GetFromJsonAsync<List<BlockstreamUtxoResponse>>(utxoUrl, cancellationToken) ?? new List<BlockstreamUtxoResponse>();

        // 3️⃣ Fetch transactions
        var txsUrl = $"{_settings.BaseUrl}/address/{address}/txs";
        var txs = await _httpClient.GetFromJsonAsync<List<BlockstreamTxResponse>>(txsUrl, cancellationToken) ?? new List<BlockstreamTxResponse>();


        // Map data into AddressModel
        var model = new AddressModel
        {
            Address = address,
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
                Confirmations = tx.status.confirmed ? tx.status.confirmations : 0,
                Timestamp = tx.status.block_time.HasValue ? DateTimeOffset.FromUnixTimeSeconds(tx.status.block_time.Value) : null,
                Inputs = tx.vin.Select(i => new TransactionInput
                {
                    PrevTxId = i.txid,
                    OutputIndex = i.vout,
                    Address = i.prevout?.scriptpubkey_address ?? string.Empty,
                    ValueSatoshi = i.prevout?.value ?? 0
                }).ToList(),
                Outputs = tx.vout.Select((o, index) => new TransactionOutput
                {
                    Address = o.scriptpubkey_address ?? string.Empty,
                    ValueSatoshi = o.value,
                    Index = index
                }).ToList()
            }).ToList()
        };

        return model;
    }
}
