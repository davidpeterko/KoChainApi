namespace KoChain.Core.Models.Bitcoin;

/// <summary>
/// Bitcoin address with balance, UTXO set, and transaction history.
/// </summary>
public class AddressModel
{
    public string Address { get; set; } = string.Empty;

    public long BalanceSatoshi { get; set; }
    public decimal BalanceBTC => BalanceSatoshi / 100_000_000m;

    public int TransactionCount { get; set; }

    public List<AddressUtxo> Utxos { get; set; } = [];
    public List<AddressTransaction> Transactions { get; set; } = [];
}

/// <summary>
/// An unspent transaction output (UTXO) belonging to an address.
/// </summary>
public class AddressUtxo
{
    public string TxId { get; set; } = string.Empty;
    public int Vout { get; set; }
    public long ValueSatoshi { get; set; }
    public decimal ValueBTC => ValueSatoshi / 100_000_000m;
    public string? ScriptPubKey { get; set; }
}

/// <summary>
/// A transaction that involves a specific address, with its full inputs and outputs.
/// </summary>
public class AddressTransaction
{
    public string TxId { get; set; } = string.Empty;
    public long FeeSatoshi { get; set; }
    public decimal FeeBTC => FeeSatoshi / 100_000_000m;

    /// <summary>0 = unconfirmed, >= 1 = confirmed.</summary>
    public int Confirmations { get; set; }
    public DateTimeOffset? Timestamp { get; set; }

    // Reuses the unified TransactionInput/TransactionOutput from TransactionModel.cs
    public List<TransactionInput> Inputs { get; set; } = [];
    public List<TransactionOutput> Outputs { get; set; } = [];
}
