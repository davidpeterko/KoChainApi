using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using NBitcoin;
using NBitcoin.RPC;

namespace KoChain.Infrastructure.Services.Rpc;

/// <summary>
/// Retrieves block data directly from a Bitcoin node via JSON-RPC.
/// <para>
/// The Bitcoin RPC node is the authoritative source for block data. Unlike address queries,
/// block lookups do not require an address index — a standard node with default settings
/// can serve all queries this service makes.
/// </para>
/// </summary>
public class RpcBlockService : IBlockService
{
    private readonly RPCClient _rpcClient;

    /// <param name="rpcClient">
    /// NBitcoin RPC client pre-configured with node credentials and network.
    /// Registered as a singleton in DI since the connection is shared across requests.
    /// </param>
    public RpcBlockService(RPCClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    /// <summary>
    /// Returns the most recently mined block on the chain tip.
    /// </summary>
    /// <remarks>
    /// Internally calls <c>getblockcount</c> to get the current height, then delegates
    /// to <see cref="GetBlockByHeightAsync"/> to fetch the block at that height.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The block at the current chain tip.</returns>
    public async Task<BlockModel> GetLatestBlockAsync(CancellationToken ct = default)
    {
        var height = await _rpcClient.GetBlockCountAsync(ct);
        return await GetBlockByHeightAsync(height, ct);
    }

    /// <summary>
    /// Returns the block at the given height in the main chain.
    /// </summary>
    /// <remarks>
    /// Calls <c>getblockhash</c> to resolve the height to a hash, then delegates
    /// to <see cref="GetBlockByHashAsync"/>. Two RPC calls total.
    /// </remarks>
    /// <param name="height">Zero-based block height (genesis block = 0).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The block at the specified height.</returns>
    public async Task<BlockModel> GetBlockByHeightAsync(int height, CancellationToken ct = default)
    {
        var hash = await _rpcClient.GetBlockHashAsync(height, ct);
        return await GetBlockByHashAsync(hash.ToString(), ct);
    }

    /// <summary>
    /// Returns the block identified by the given hash.
    /// </summary>
    /// <remarks>
    /// Uses <c>getblock</c> with verbosity level 1, which returns a JSON object containing
    /// both the block header fields (height, time, previousblockhash) and summary fields
    /// (nTx, size). This avoids a separate <c>getblockheader</c> call that would otherwise
    /// be needed to retrieve the height when starting from a hash.
    /// </remarks>
    /// <param name="hash">The block hash as a hex string.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The block with the given hash.</returns>
    /// <exception cref="NBitcoin.RPC.RPCException">
    /// Thrown by NBitcoin if the hash does not exist on the node.
    /// </exception>
    public async Task<BlockModel> GetBlockByHashAsync(string hash, CancellationToken ct = default)
    {
        // verbosity=1 returns JSON with all header fields + nTx + size.
        // verbosity=0 returns only the raw hex — no metadata.
        // verbosity=2 would include full transaction objects, which we don't need here.
        var result = await _rpcClient.SendCommandAsync(RPCOperations.getblock, hash, 1);
        var json = result.Result;

        return new BlockModel
        {
            Hash = hash,
            Height = (int)json["height"]!,
            // Bitcoin stores block time as Unix seconds (uint32). The earliest possible
            // value is the genesis block timestamp (2009-01-03), so no overflow risk.
            Time = DateTimeOffset.FromUnixTimeSeconds((long)json["time"]!).UtcDateTime,
            TransactionCount = (int)json["nTx"]!,
            // Genesis block (height 0) has no previous block, so this field is absent.
            PreviousBlockHash = (string?)json["previousblockhash"] ?? string.Empty,
            Size = (int)json["size"]!
        };
    }
}
