namespace KoChain.Core.Models.Bitcoin.Transaction;

public class TransactionModel
{
    /// <summary>
    /// The transaction ID (hash) uniquely identifying this transaction.
    /// </summary>
    public string TxId { get; set; } = string.Empty;

    /// <summary>
    /// Total value of all outputs in the transaction, in BTC.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// List of inputs (UTXOs being spent)
    /// </summary>
    public List<TransactionInput> Inputs { get; set; } = [];

    /// <summary>
    /// List of outputs (where BTC is sent)
    /// </summary>
    public List<TransactionOutput> Outputs { get; set; } = [];
}

public class TransactionInput
{
    /// <summary>
    /// Previous transaction hash (UTXO being spent)
    /// </summary>
    public string PrevTxId { get; set; } = string.Empty;

    /// <summary>
    /// Output index in the previous transaction
    /// </summary>
    public int OutputIndex { get; set; }

    /// <summary>
    /// Address providing the funds (derived from previous output)
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Value of this input in BTC (derived from previous output)
    /// </summary>
    public decimal Value { get; set; }
}

public class TransactionOutput
{
    /// <summary>
    /// Address receiving funds
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Value sent to this address in BTC
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Index of this output in the transaction
    /// </summary>
    public int Index { get; set; }
}
