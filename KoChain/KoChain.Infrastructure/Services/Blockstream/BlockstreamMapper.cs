using KoChain.Core.Models.Bitcoin;

namespace KoChain.Infrastructure.Services.Blockstream;

/// <summary>
/// Shared mapping logic from raw Blockstream API response types to domain models.
/// Centralised here so that BlockstreamTransactionService, BlockstreamAddressService,
/// and BlockstreamMempoolService all produce consistent output from the same API shape.
/// </summary>
internal static class BlockstreamMapper
{
    internal static TransactionModel ToTransactionModel(BlockstreamTxResponse tx) => new()
    {
        TxId = tx.txid,
        FeeSatoshi = tx.fee,
        Confirmations = tx.status.confirmed ? 1 : 0,
        BlockHeight = tx.status.block_height,
        Timestamp = tx.status.block_time.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(tx.status.block_time.Value)
            : null,
        Inputs = MapInputs(tx.vin),
        Outputs = MapOutputs(tx.vout)
    };

    internal static AddressTransaction ToAddressTransaction(BlockstreamTxResponse tx) => new()
    {
        TxId = tx.txid,
        FeeSatoshi = tx.fee,
        Confirmations = tx.status.confirmed ? 1 : 0,
        Timestamp = tx.status.block_time.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(tx.status.block_time.Value)
            : null,
        Inputs = MapInputs(tx.vin),
        Outputs = MapOutputs(tx.vout)
    };

    private static List<TransactionInput> MapInputs(List<Vin> vin) =>
        vin.Select(i => new TransactionInput
        {
            PrevTxId = i.txid,
            OutputIndex = i.vout,
            // prevout is null for coinbase inputs (block reward) — no previous output exists.
            Address = i.prevout?.scriptpubkey_address ?? string.Empty,
            ValueSatoshi = i.prevout?.value ?? 0
        }).ToList();

    private static List<TransactionOutput> MapOutputs(List<Vout> vout) =>
        vout.Select((o, index) => new TransactionOutput
        {
            // Null for unspendable outputs such as OP_RETURN data carriers.
            Address = o.scriptpubkey_address ?? string.Empty,
            ValueSatoshi = o.value,
            Index = index
        }).ToList();
}
