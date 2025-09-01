using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoChain.Core.Models.Bitcoin;

public class BlockModel
{
    /// <summary>
    /// The block hash as a string
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Block height in the blockchain
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Timestamp of the block
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Number of transactions in the block
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Optional: Previous block hash
    /// </summary>
    public string PreviousBlockHash { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Size of the block in bytes
    /// </summary>
    public int Size { get; set; }
}
