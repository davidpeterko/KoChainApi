using KoChain.Core.Models.Bitcoin.Address;

namespace KoChain.Core.Interfaces;

public interface IAddressService
{
    Task<AddressModel> GetAddressDataAsync(string address, CancellationToken cancellationToken = default);
}
