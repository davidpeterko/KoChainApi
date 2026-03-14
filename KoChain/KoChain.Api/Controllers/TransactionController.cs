using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace KoChain.Api.Controllers;

/// <summary>
/// Endpoints for querying Bitcoin transactions.
/// Transaction data is served from the Blockstream Esplora API, which returns
/// fully enriched input and output data (addresses, amounts) in a single call.
/// </summary>
[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Returns full details for a transaction, including all inputs and outputs.
    /// </summary>
    /// <param name="txid">The transaction ID (txid) as a 64-character hex string.</param>
    /// <remarks>
    /// Each input includes the source address and amount (resolved from the previous output).
    /// Each output includes the destination address and amount.
    /// Coinbase inputs (block reward) will have an empty address and zero value,
    /// as they have no previous output to reference.
    /// </remarks>
    /// <response code="200">The transaction with fully populated inputs and outputs.</response>
    /// <response code="400">The value is not a valid 256-bit transaction ID.</response>
    /// <response code="404">No transaction with the given txid was found.</response>
    /// <response code="500">The upstream API is unreachable or returned an error.</response>
    [HttpGet("{txid}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransaction(string txid, CancellationToken ct)
    {
        // uint256.TryParse validates that the value is a properly formatted 256-bit hex hash.
        if (!uint256.TryParse(txid, out _))
            return Problem(
                detail: $"'{txid}' is not a valid transaction ID. Expected a 64-character hex string.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid transaction ID.");

        var transaction = await _transactionService.GetTransactionAsync(txid, ct);
        return Ok(transaction);
    }
}
