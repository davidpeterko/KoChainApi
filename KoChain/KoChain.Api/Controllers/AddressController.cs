using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace KoChain.Api.Controllers;

/// <summary>
/// Endpoints for querying Bitcoin addresses.
/// Address data is served from the Blockstream Esplora API, which maintains
/// an address index — something a bare Bitcoin node does not provide.
/// </summary>
[ApiController]
[Route("api/addresses")]
[Produces("application/json")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly Network _network;

    public AddressController(IAddressService addressService, Network network)
    {
        _addressService = addressService;
        _network = network;
    }

    /// <summary>
    /// Returns a full snapshot of a Bitcoin address: balance, UTXOs, and first page of transactions.
    /// </summary>
    /// <param name="address">
    /// A Bitcoin address in any standard format: legacy (1...), P2SH (3...), or bech32 (bc1...).
    /// </param>
    /// <remarks>
    /// Balance is calculated as total received minus total spent, in satoshis.
    /// Transaction history shows the most recent 25 transactions.
    /// For full paginated history use <c>GET /api/addresses/{address}/transactions</c>.
    /// UTXOs represent outputs that are currently spendable by this address.
    /// </remarks>
    /// <response code="200">The address with balance, UTXOs, and recent transactions.</response>
    /// <response code="400">The address is not a valid Bitcoin address for the configured network.</response>
    /// <response code="404">The address has no on-chain activity or does not exist.</response>
    /// <response code="500">The upstream API is unreachable or returned an error.</response>
    [HttpGet("{address}")]
    [ProducesResponseType(typeof(AddressModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAddress(string address, CancellationToken ct)
    {
        // BitcoinAddress.Create validates the full address: format, checksum, and network prefix.
        // This catches malformed input early and returns a clear 400 rather than leaking
        // a confusing upstream error from Blockstream.
        try
        {
            BitcoinAddress.Create(address, _network);
        }
        catch (FormatException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Bitcoin address.");
        }

        var addressData = await _addressService.GetAddressDataAsync(address, ct);
        return Ok(addressData);
    }

    /// <summary>
    /// Returns a page of transactions for an address, supporting cursor-based pagination.
    /// </summary>
    /// <param name="address">The Bitcoin address to query.</param>
    /// <param name="after">
    /// The txid of the last transaction from the previous page. Omit for the first page.
    /// </param>
    /// <remarks>
    /// To paginate, pass the txid of the last transaction from the current response as
    /// the <c>after</c> parameter. Each page returns up to 25 transactions ordered newest
    /// to oldest. When fewer than 25 are returned, you have reached the end.
    /// </remarks>
    /// <response code="200">Up to 25 transactions, ordered newest to oldest.</response>
    /// <response code="400">The address is invalid or the after cursor is malformed.</response>
    /// <response code="404">The address has no on-chain activity.</response>
    /// <response code="500">The upstream API is unreachable or returned an error.</response>
    [HttpGet("{address}/transactions")]
    [ProducesResponseType(typeof(List<AddressTransaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAddressTransactions(string address, [FromQuery] string? after = null, CancellationToken ct = default)
    {
        try
        {
            BitcoinAddress.Create(address, _network);
        }
        catch (FormatException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Bitcoin address.");
        }

        if (after is not null && !uint256.TryParse(after, out _))
            return Problem(
                detail: $"'{after}' is not a valid transaction ID. The 'after' cursor must be a 64-character hex string.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid pagination cursor.");

        var transactions = await _addressService.GetAddressTransactionsAsync(address, after, ct);
        return Ok(transactions);
    }
}
