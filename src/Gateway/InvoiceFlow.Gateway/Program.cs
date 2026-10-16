using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.Gateway;

var builder = WebApplication.CreateBuilder(args);

builder.AddInvoiceFlowObservability(builder.Environment.ApplicationName);

builder.Services.AddServiceDiscovery();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddGatewayRateLimiting();

var app = builder.Build();

app.UseRouting();

app.UseRateLimiter();

app.MapReverseProxy();

app.Run();
