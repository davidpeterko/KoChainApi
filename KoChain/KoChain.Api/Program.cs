using KoChain.Core.Interfaces;
using KoChain.Infrastructure.Configuration;
using KoChain.Infrastructure.Services.Blockstream;
using KoChain.Infrastructure.Services.Rpc;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<BitcoinRpcSettings>(builder.Configuration.GetSection("BitcoinRpcSettings"));
builder.Services.Configure<BlockstreamSettings>(builder.Configuration.GetSection("Blockstream"));

// Bitcoin RPC client (singleton — one connection to the node)
builder.Services.AddSingleton<RPCClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<BitcoinRpcSettings>>().Value;
    var creds = new NetworkCredential(options.User, options.Password);
    var uri = new Uri(options.Url);

    Network network = options.Network.ToLower() switch
    {
        "main" => Network.Main,
        "testnet" => Network.TestNet,
        "regtest" => Network.RegTest,
        _ => Network.Main
    };

    return new RPCClient(creds, uri, network);
});

// IBlockService  → RPC node (authoritative for block data, no address index needed)
builder.Services.AddScoped<IBlockService, RpcBlockService>();

// ITransactionService → Blockstream (one HTTP call returns full input/output data)
builder.Services.AddHttpClient<ITransactionService, BlockstreamTransactionService>();

// IAddressService → Blockstream (bare RPC node has no address index)
builder.Services.AddHttpClient<IAddressService, BlockstreamAddressService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
