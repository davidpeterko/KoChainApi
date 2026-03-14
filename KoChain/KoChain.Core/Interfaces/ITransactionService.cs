using KoChain.Core.Models.Bitcoin;

namespace KoChain.Core.Interfaces;

public interface ITransactionService
{
    /// <summary>Returns full details for a single transaction by txid.</summary>
    Task<TransactionModel> GetTransactionAsync(string txId, CancellationToken ct = default);

    /// <summary>
    /// Returns a page of transactions contained in the given block.
    /// Each page contains up to 25 transactions. Use <paramref name="page"/> to paginate
    /// (0 = first 25, 1 = next 25, etc.).
    /// </summary>
    Task<List<TransactionModel>> GetBlockTransactionsAsync(string blockHash, int page = 0, CancellationToken ct = default);
}
