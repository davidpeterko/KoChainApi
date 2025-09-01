using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoChain.Infrastructure.Configuration;

public class BitcoinRpcSettings
{
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
}
