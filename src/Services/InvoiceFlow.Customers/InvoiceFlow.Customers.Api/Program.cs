using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.BuildingBlocks.Authorization.ExtensionMethods;
using InvoiceFlow.Customers.Application;
using InvoiceFlow.Customers.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting up");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddInvoiceFlowAuthentication(builder.Configuration);
    builder.Services.AddInvoiceFlowAuthorization();

    builder.RegisterApplicationServices();
    builder.RegisterInfrastructureServices();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseValidationExceptionHandler();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.ConfigureInfrastructureServices();

    await app.RunAsync();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.Information("Shutting down");
    await Log.CloseAndFlushAsync();
}
