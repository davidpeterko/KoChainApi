using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using Microsoft.AspNetCore.Mvc;

namespace KoChain.Api.Controllers;

/// <summary>
/// Endpoints for querying the Bitcoin mempool (unconfirmed transaction pool).
/// Data is served from Blockstream, which indexes the mempool in real time.
/// </summary>
[ApiController]
[Route("api/mempool")]
[Produces("application/json")]
public class MempoolController : ControllerBase
{
    private readonly IMempoolService _mempoolService;

    public MempoolController(IMempoolService mempoolService)
    {
        _mempoolService = mempoolService;
    }

    /// <summary>
    /// Returns aggregate statistics about the current mempool state.
    /// </summary>
    /// <remarks>
    /// High transaction count and virtual size indicate a congested mempool.
    /// During congestion, users must offer higher fees to get their transactions confirmed quickly.
    /// </remarks>
    /// <response code="200">Transaction count, total virtual size in vbytes, and total fees.</response>
    /// <response code="500">The upstream API is unreachable or returned an error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(MempoolStats), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMempoolStats(CancellationToken ct)
    {
        var stats = await _mempoolService.GetMempoolStatsAsync(ct);
        return Ok(stats);
    }

    /// <summary>
    /// Returns the most recently submitted unconfirmed transactions (up to 10).
    /// </summary>
    /// <remarks>
    /// Useful for showing live network activity on an explorer homepage.
    /// These transactions are in the mempool and have not yet been included in a block.
    /// </remarks>
    /// <response code="200">Up to 10 of the most recently submitted unconfirmed transactions.</response>
    /// <response code="500">The upstream API is unreachable or returned an error.</response>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentTransactions(CancellationToken ct)
    {
        var transactions = await _mempoolService.GetRecentTransactionsAsync(ct);
        return Ok(transactions);
    }
}
