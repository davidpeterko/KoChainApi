namespace KoChain.Core.Models.Bitcoin;

/// <summary>
/// Summary statistics about the current Bitcoin mempool (unconfirmed transaction pool).
/// </summary>
public class MempoolStats
{
    /// <summary>Number of unconfirmed transactions currently in the mempool.</summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Total virtual size of all mempool transactions in vbytes.
    /// Virtual size accounts for SegWit discount and is what miners use to calculate fees.
    /// </summary>
    public long VirtualSizeBytes { get; set; }

    /// <summary>Total fees offered by all mempool transactions, in satoshis.</summary>
    public long TotalFeeSatoshi { get; set; }

    public decimal TotalFeeBTC => TotalFeeSatoshi / 100_000_000m;
}
