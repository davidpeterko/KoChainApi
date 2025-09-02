using KoChain.Core.Interfaces;
using KoChain.Infrastructure.Configuration;
using KoChain.Infrastructure.Services.Rpc;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// NBitcoin RPC client configuration
builder.Services.Configure<BitcoinRpcSettings>(
    builder.Configuration.GetSection("BitcoinRpcSettings"));

// Blockstream API configuration
builder.Services.Configure<BlockstreamSettings>(
    builder.Configuration.GetSection("Blockstream"));

// Register RPCClient as singleton
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

// Register our service: interface -> concrete implementation
builder.Services.AddScoped<IBlockchainService, RpcBlockchainService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
