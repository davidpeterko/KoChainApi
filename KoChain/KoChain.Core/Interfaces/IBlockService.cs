using KoChain.Core.Models.Bitcoin;

namespace KoChain.Core.Interfaces;

public interface IBlockService
{
    Task<BlockModel> GetLatestBlockAsync(CancellationToken ct = default);
    Task<BlockModel> GetBlockByHeightAsync(int height, CancellationToken ct = default);
    Task<BlockModel> GetBlockByHashAsync(string hash, CancellationToken ct = default);
}
