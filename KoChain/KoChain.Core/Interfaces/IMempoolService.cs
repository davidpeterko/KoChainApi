using KoChain.Core.Models.Bitcoin;

namespace KoChain.Core.Interfaces;

public interface IMempoolService
{
    /// <summary>Returns aggregate statistics about the current mempool.</summary>
    Task<MempoolStats> GetMempoolStatsAsync(CancellationToken ct = default);

    /// <summary>Returns the most recently added unconfirmed transactions (up to 10).</summary>
    Task<List<TransactionModel>> GetRecentTransactionsAsync(CancellationToken ct = default);
}
