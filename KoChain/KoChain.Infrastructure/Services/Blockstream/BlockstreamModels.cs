namespace KoChain.Infrastructure.Services.Blockstream;

// Raw JSON response shapes from the Blockstream Esplora API.
// These are infrastructure-only — nothing in Core should reference them.

internal class BlockstreamAddressResponse
{
    public ChainStats chain_stats { get; set; } = new();
}

internal class ChainStats
{
    public long funded_txo_sum { get; set; }
    public long spent_txo_sum { get; set; }
    public int tx_count { get; set; }
}

internal class BlockstreamUtxoResponse
{
    public string txid { get; set; } = string.Empty;
    public int vout { get; set; }
    public long value { get; set; }
    public string scriptpubkey { get; set; } = string.Empty;
}

internal class BlockstreamTxResponse
{
    public string txid { get; set; } = string.Empty;
    public long fee { get; set; }
    public TxStatus status { get; set; } = new();
    public List<Vin> vin { get; set; } = [];
    public List<Vout> vout { get; set; } = [];
}

internal class TxStatus
{
    public bool confirmed { get; set; }
    public int? block_height { get; set; }
    public long? block_time { get; set; }
}

internal class Vin
{
    public string txid { get; set; } = string.Empty;
    public int vout { get; set; }
    public Prevout? prevout { get; set; }
}

internal class Prevout
{
    public string? scriptpubkey_address { get; set; }
    public long value { get; set; }
}

internal class Vout
{
    public string? scriptpubkey_address { get; set; }
    public long value { get; set; }
}
