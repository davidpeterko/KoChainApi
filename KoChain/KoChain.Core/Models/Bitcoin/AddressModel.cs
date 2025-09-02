namespace KoChain.Core.Models.Bitcoin.Address;

/// <summary>
/// Represents a Bitcoin address with balance, transaction history, and unspent outputs (UTXOs).
/// </summary>
public class AddressModel
{
    /// <summary>
    /// The Bitcoin address string.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Balance of the address in satoshis.
    /// </summary>
    public long BalanceSatoshi { get; set; }

    /// <summary>
    /// Balance of the address in Bitcoin (BTC).
    /// </summary>
    public decimal BalanceBTC => BalanceSatoshi / 100_000_000m;

    /// <summary>
    /// Number of transactions involving this address.
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// List of unspent transaction outputs (UTXOs) for this address.
    /// </summary>
    public List<AddressUtxo> Utxos { get; set; } = new();

    /// <summary>
    /// List of transactions associated with this address.
    /// </summary>
    public List<AddressTransaction> Transactions { get; set; } = new();
}

/// <summary>
/// Represents a single unspent transaction output (UTXO) for a Bitcoin address.
/// </summary>
public class AddressUtxo
{
    /// <summary>
    /// Transaction ID of the UTXO.
    /// </summary>
    public string TxId { get; set; } = string.Empty;

    /// <summary>
    /// Output index in the transaction.
    /// </summary>
    public int Vout { get; set; }

    /// <summary>
    /// Amount of the UTXO in satoshis.
    /// </summary>
    public long ValueSatoshi { get; set; }

    /// <summary>
    /// Amount of the UTXO in Bitcoin (BTC).
    /// </summary>
    public decimal ValueBTC => ValueSatoshi / 100_000_000m;

    /// <summary>
    /// Optional ScriptPubKey of the UTXO (useful for advanced info).
    /// </summary>
    public string? ScriptPubKey { get; set; }
}

/// <summary>
/// Represents a transaction associated with a Bitcoin address.
/// </summary>
public class AddressTransaction
{
    /// <summary>
    /// Transaction ID.
    /// </summary>
    public string TxId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction fee in satoshis.
    /// </summary>
    public long FeeSatoshi { get; set; }

    /// <summary>
    /// Transaction fee in Bitcoin (BTC).
    /// </summary>
    public decimal FeeBTC => FeeSatoshi / 100_000_000m;

    /// <summary>
    /// Number of confirmations for this transaction.
    /// </summary>
    public int Confirmations { get; set; }

    /// <summary>
    /// Timestamp when the transaction was included in a block (if confirmed).
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// List of inputs in the transaction.
    /// </summary>
    public List<TransactionInput> Inputs { get; set; } = new();

    /// <summary>
    /// List of outputs in the transaction.
    /// </summary>
    public List<TransactionOutput> Outputs { get; set; } = new();
}

/// <summary>
/// Represents a transaction input for a Bitcoin transaction.
/// </summary>
public class TransactionInput
{
    /// <summary>
    /// Transaction ID of the previous output being spent.
    /// </summary>
    public string PrevTxId { get; set; } = string.Empty;

    /// <summary>
    /// Output index of the previous transaction.
    /// </summary>
    public int OutputIndex { get; set; }

    /// <summary>
    /// Address of the previous output.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Value of the input in satoshis.
    /// </summary>
    public long ValueSatoshi { get; set; }

    /// <summary>
    /// Value of the input in Bitcoin (BTC).
    /// </summary>
    public decimal ValueBTC => ValueSatoshi / 100_000_000m;
}

/// <summary>
/// Represents a transaction output for a Bitcoin transaction.
/// </summary>
public class TransactionOutput
{
    /// <summary>
    /// Receiving address for this output.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Value of the output in satoshis.
    /// </summary>
    public long ValueSatoshi { get; set; }

    /// <summary>
    /// Value of the output in Bitcoin (BTC).
    /// </summary>
    public decimal ValueBTC => ValueSatoshi / 100_000_000m;

    /// <summary>
    /// Index of this output in the transaction.
    /// </summary>
    public int Index { get; set; }
}
