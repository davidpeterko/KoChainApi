using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace KoChain.Api.Controllers;

/// <summary>
/// Endpoints for querying Bitcoin blocks.
/// Block metadata is served from the local RPC node.
/// Block transaction lists are served from Blockstream, which returns enriched data.
/// </summary>
[ApiController]
[Route("api/blocks")]
[Produces("application/json")]
public class BlockController : ControllerBase
{
    private readonly IBlockService _blockService;
    private readonly ITransactionService _transactionService;

    public BlockController(IBlockService blockService, ITransactionService transactionService)
    {
        _blockService = blockService;
        _transactionService = transactionService;
    }

    /// <summary>
    /// Returns the current chain tip height.
    /// Cheaper than fetching the full latest block — useful for calculating confirmation counts.
    /// </summary>
    /// <response code="200">The current block height as an integer.</response>
    /// <response code="500">The RPC node is unreachable or returned an error.</response>
    [HttpGet("tip")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChainTip(CancellationToken ct)
    {
        var height = await _blockService.GetChainTipHeightAsync(ct);
        return Ok(new { height });
    }

    /// <summary>
    /// Returns the most recently mined block at the current chain tip.
    /// </summary>
    /// <response code="200">The latest block.</response>
    /// <response code="500">The RPC node is unreachable or returned an error.</response>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(BlockModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLatestBlock(CancellationToken ct)
    {
        var block = await _blockService.GetLatestBlockAsync(ct);
        return Ok(block);
    }

    /// <summary>
    /// Returns the block at the given height in the main chain.
    /// </summary>
    /// <param name="height">Zero-based block height. The genesis block is height 0.</param>
    /// <response code="200">The block at the specified height.</response>
    /// <response code="400">Height is negative.</response>
    /// <response code="404">No block exists at the given height (beyond the chain tip).</response>
    /// <response code="500">The RPC node is unreachable or returned an error.</response>
    [HttpGet("height/{height:int}")]
    [ProducesResponseType(typeof(BlockModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBlockByHeight(int height, CancellationToken ct)
    {
        if (height < 0)
            return Problem(
                detail: $"Height '{height}' is invalid. Block height must be a non-negative integer.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid block height.");

        var block = await _blockService.GetBlockByHeightAsync(height, ct);
        return Ok(block);
    }

    /// <summary>
    /// Returns the block identified by its hash.
    /// </summary>
    /// <param name="hash">The block hash as a 64-character hex string.</param>
    /// <response code="200">The block with the given hash.</response>
    /// <response code="400">The value is not a valid 256-bit block hash.</response>
    /// <response code="404">No block with the given hash exists on this node.</response>
    /// <response code="500">The RPC node is unreachable or returned an error.</response>
    [HttpGet("{hash}")]
    [ProducesResponseType(typeof(BlockModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBlockByHash(string hash, CancellationToken ct)
    {
        if (!uint256.TryParse(hash, out _))
            return Problem(
                detail: $"'{hash}' is not a valid 256-bit block hash. Expected a 64-character hex string.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid block hash.");

        var block = await _blockService.GetBlockByHashAsync(hash, ct);
        return Ok(block);
    }

    /// <summary>
    /// Returns a page of transactions from the given block.
    /// </summary>
    /// <param name="hash">The block hash as a 64-character hex string.</param>
    /// <param name="page">Zero-based page number. Each page contains up to 25 transactions.</param>
    /// <remarks>
    /// Transactions are returned in the order they appear in the block.
    /// To get all transactions in a large block, increment the page number until fewer
    /// than 25 transactions are returned.
    /// </remarks>
    /// <response code="200">Up to 25 transactions from the specified page of the block.</response>
    /// <response code="400">The hash is invalid or the page number is negative.</response>
    /// <response code="404">No block with the given hash was found.</response>
    /// <response code="500">The upstream API is unreachable or returned an error.</response>
    [HttpGet("{hash}/transactions")]
    [ProducesResponseType(typeof(List<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBlockTransactions(string hash, [FromQuery] int page = 0, CancellationToken ct = default)
    {
        if (!uint256.TryParse(hash, out _))
            return Problem(
                detail: $"'{hash}' is not a valid 256-bit block hash. Expected a 64-character hex string.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid block hash.");

        if (page < 0)
            return Problem(
                detail: "Page number must be a non-negative integer.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid page number.");

        var transactions = await _transactionService.GetBlockTransactionsAsync(hash, page, ct);
        return Ok(transactions);
    }
}
