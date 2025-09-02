namespace KoChain.Core.Models.Blockstream;

public class BlockstreamAddressResponse
{
    public ChainStats chain_stats { get; set; } = new();
}

public class ChainStats
{
    public long funded_txo_sum { get; set; }
    public long spent_txo_sum { get; set; }
    public int tx_count { get; set; }
}

public class BlockstreamUtxoResponse
{
    public string txid { get; set; } = string.Empty;
    public int vout { get; set; }
    public long value { get; set; }
    public string scriptpubkey { get; set; } = string.Empty;
}

public class BlockstreamTxResponse
{
    public string txid { get; set; } = string.Empty;
    public long fee { get; set; }
    public TxStatus status { get; set; } = new();
    public List<Vin> vin { get; set; } = new();
    public List<Vout> vout { get; set; } = new();
}

public class TxStatus
{
    public bool confirmed { get; set; }
    public int confirmations { get; set; }
    public long? block_time { get; set; }
}

public class Vin
{
    public string txid { get; set; } = string.Empty;
    public int vout { get; set; }
    public Prevout? prevout { get; set; }
}

public class Prevout
{
    public string scriptpubkey_address { get; set; } = string.Empty;
    public long value { get; set; }
}

public class Vout
{
    public string? scriptpubkey_address { get; set; }
    public long value { get; set; }
}