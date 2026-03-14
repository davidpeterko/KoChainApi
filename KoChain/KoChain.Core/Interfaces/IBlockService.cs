using KoChain.Core.Models.Bitcoin;

namespace KoChain.Core.Interfaces;

public interface IBlockService
{
    /// <summary>Returns the current chain tip height without fetching the full block.</summary>
    Task<int> GetChainTipHeightAsync(CancellationToken ct = default);

    Task<BlockModel> GetLatestBlockAsync(CancellationToken ct = default);
    Task<BlockModel> GetBlockByHeightAsync(int height, CancellationToken ct = default);
    Task<BlockModel> GetBlockByHashAsync(string hash, CancellationToken ct = default);
}
