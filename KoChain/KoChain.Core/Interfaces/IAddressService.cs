using KoChain.Core.Models.Bitcoin;

namespace KoChain.Core.Interfaces;

public interface IAddressService
{
    Task<AddressModel> GetAddressDataAsync(string address, CancellationToken ct = default);
}
