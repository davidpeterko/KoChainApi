using NBitcoin;
using NBitcoin.RPC;
using KoChain.Core.Interfaces;
using KoChain.Core.Models.Bitcoin;

namespace KoChain.Infrastructure.Services;

public class RpcBlockchainService : IBlockchainService
{
    private readonly RPCClient _rpcClient;

    public RpcBlockchainService(RPCClient rpcClient)
    {
        _rpcClient = rpcClient;
    }   

    public async Task<BlockModel> GetLatestBlockAsync(CancellationToken ct = default)
    {
        var height = await _rpcClient.GetBlockCountAsync(ct);
        return await GetBlockByHeightAsync(height, ct);
    }

    public async Task<BlockModel> GetBlockByHeightAsync(int height, CancellationToken ct = default)
    {
        var hash = await _rpcClient.GetBlockHashAsync(height, ct);
        var block = await _rpcClient.GetBlockAsync(hash, ct);

        return new BlockModel
        {
            Hash = hash.ToString(),
            Height = height,
            Time = block.Header.BlockTime.UtcDateTime,
            TransactionCount = block.Transactions.Count,
            PreviousBlockHash = block.Header.HashPrevBlock.ToString(),
            Size = block.GetSerializedSize()
        };
    }

    public async Task<TransactionModel> GetTransactionAsync(string txId, CancellationToken ct = default)
    {
        var txIdObj = uint256.Parse(txId);
        var tx = await _rpcClient.GetRawTransactionAsync(txIdObj, true, ct);

        // Get sum of all output from transaction
        var totalTransactionAmount = tx.Outputs.Sum(o => o.Value.ToDecimal(MoneyUnit.BTC));

        // Get all input (from) transactions hashes
        // Bitcoin uses a UTXO (unspent transaction output) model, each input references a previous transaction output (PrevOut)
        // That output may have been sent to one or more addresses
        var inputs = new List<TransactionInput>();

        foreach (var input in tx.Inputs)
        {
            // Get the previous transaction this input is spending from
            var previousTx = await _rpcClient.GetRawTransactionAsync(input.PrevOut.Hash, true, ct);
            var previousOuput = previousTx.Outputs[input.PrevOut.N];

            // Get the address associated with this input
            // Null-coalescing to empty string if address extraction fails (reasoning is that it could be a non-standard address)
            var address = previousOuput.ScriptPubKey.GetDestinationAddress(_rpcClient.Network)?.ToString() ?? "";

            inputs.Add(new TransactionInput
            {
                PrevTxId = input.PrevOut.Hash.ToString(),
                OutputIndex = (int)input.PrevOut.N,
                Address = address,
                Value = previousOuput.Value.ToDecimal(MoneyUnit.BTC)
            });
        }

        // Map transaciton outputs to transaction output models
        var outputs = tx.Outputs.Select((o, index) => new TransactionOutput
        {
            // Extract the receiving address for this output, or empty string if unavailable
            Address = o.ScriptPubKey.GetDestinationAddress(_rpcClient.Network)?.ToString() ?? "",

            // Value in BTC sent to this address
            Value = o.Value.ToDecimal(MoneyUnit.BTC),

            // Index of this output in the current transaction
            Index = index
        }).ToList();

        return new TransactionModel
        {
            TxId = txId,
            Amount = totalTransactionAmount,
            Inputs = inputs,
            Outputs = outputs
        };
    }
}
