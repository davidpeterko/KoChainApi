using KoChain.Core.Models.Bitcoin;

namespace KoChain.Core.Interfaces;

public interface ITransactionService
{
    Task<TransactionModel> GetTransactionAsync(string txId, CancellationToken ct = default);
}
