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
    /// Returns a full snapshot of a Bitcoin address: balance, UTXOs, and transaction history.b
    /// </summary>
    /// <param name="address">
    /// A Bitcoin address in any standard format: legacy (1...), P2SH (3...), or bech32 (bc1...).
    /// </param>
    /// <remarks>
    /// Balance is calculated as total received minus total spent, in satoshis.
    /// Transaction history is limited to the 25 most recent transactions by the upstream API.
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
}
