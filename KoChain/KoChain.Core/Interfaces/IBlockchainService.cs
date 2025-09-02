using KoChain.Core.Models.Bitcoin;
using KoChain.Core.Models.Bitcoin.Transaction;

namespace KoChain.Core.Interfaces;

public interface IBlockchainService
{
    Task<BlockModel> GetLatestBlockAsync(CancellationToken ct = default);
    Task<BlockModel> GetBlockByHeightAsync(int height, CancellationToken ct = default);
    Task<TransactionModel> GetTransactionAsync(string txId, CancellationToken ct = default);
}
