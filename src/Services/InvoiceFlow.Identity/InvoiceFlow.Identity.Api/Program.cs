using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.BuildingBlocks.Authorization.ExtensionMethods;
using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddInvoiceFlowExceptionHandling();
    builder.AddInvoiceFlowObservability(builder.Environment.ApplicationName);

    builder.Services.AddInvoiceFlowAuthentication(builder.Configuration);
    builder.Services.AddInvoiceFlowAuthorization();

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddHttpContextAccessor();

    builder.Services
        .AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
        .AddDbContextCheck<IdentityDbContext>(name: "identity-database", tags: ["ready"]);

    var app = builder.Build();

    _ = app.Services.GetRequiredService<IOptions<JwtSettings>>().Value;

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseExceptionHandler();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = (check) => check.Tags.Contains("live")
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = (check) => check.Tags.Contains("ready")
    });

    if (app.Environment.IsDevelopment())
    {
        await app.Services.ConfigureInfrastructureServicesAsync();
    }

    await app.RunAsync();

}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Application terminated unexpectedly: {ex}");

    throw;
}
