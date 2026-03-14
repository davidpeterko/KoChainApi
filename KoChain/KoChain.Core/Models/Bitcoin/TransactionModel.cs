namespace KoChain.Core.Models.Bitcoin;

public class TransactionModel
{
    /// <summary>Transaction ID (hash).</summary>
    public string TxId { get; set; } = string.Empty;

    /// <summary>Transaction fee in satoshis.</summary>
    public long FeeSatoshi { get; set; }

    public decimal FeeBTC => FeeSatoshi / 100_000_000m;

    /// <summary>Sum of all outputs in satoshis.</summary>
    public long AmountSatoshi => Outputs.Sum(o => o.ValueSatoshi);

    public decimal AmountBTC => AmountSatoshi / 100_000_000m;

    /// <summary>Number of confirmations. 0 = unconfirmed.</summary>
    public int Confirmations { get; set; }

    /// <summary>Block height this transaction was included in, if confirmed.</summary>
    public int? BlockHeight { get; set; }

    /// <summary>Timestamp when the transaction was confirmed.</summary>
    public DateTimeOffset? Timestamp { get; set; }

    public List<TransactionInput> Inputs { get; set; } = [];
    public List<TransactionOutput> Outputs { get; set; } = [];
}

public class TransactionInput
{
    /// <summary>Hash of the previous transaction being spent.</summary>
    public string PrevTxId { get; set; } = string.Empty;

    /// <summary>
    /// Output index in the previous transaction.
    /// For coinbase inputs this will be 4294967295 (0xFFFFFFFF), the Bitcoin protocol
    /// sentinel value indicating there is no previous output.
    /// </summary>
    public long OutputIndex { get; set; }

    /// <summary>Address that owns this input (derived from the previous output).</summary>
    public string Address { get; set; } = string.Empty;

    public long ValueSatoshi { get; set; }
    public decimal ValueBTC => ValueSatoshi / 100_000_000m;
}

public class TransactionOutput
{
    /// <summary>Address receiving the funds.</summary>
    public string Address { get; set; } = string.Empty;

    public long ValueSatoshi { get; set; }
    public decimal ValueBTC => ValueSatoshi / 100_000_000m;

    /// <summary>Index of this output within the transaction.</summary>
    public int Index { get; set; }
}
