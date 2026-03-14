using KoChain.Core.Models.Bitcoin;

namespace KoChain.Core.Interfaces;

public interface IAddressService
{
    /// <summary>Returns balance, UTXOs, and the first page of transactions for an address.</summary>
    Task<AddressModel> GetAddressDataAsync(string address, CancellationToken ct = default);

    /// <summary>
    /// Returns a page of transactions for an address.
    /// Pass <paramref name="afterTxId"/> (the last txid from the previous page) to get the next page.
    /// Omit it to get the most recent transactions.
    /// Each page contains up to 25 transactions.
    /// </summary>
    Task<List<AddressTransaction>> GetAddressTransactionsAsync(string address, string? afterTxId = null, CancellationToken ct = default);
}
