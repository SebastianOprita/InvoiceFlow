using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.ExtensionMethods;
using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

try
{
    var builder = WebApplication.CreateBuilder(args);

    if (builder.Configuration["JwtSettings:Secret"] is null)
    {
        throw new InvalidOperationException("JwtSettings:Secret is not configured.");
    }

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddInvoiceFlowExceptionHandling();
    builder.AddInvoiceFlowObservability(builder.Environment.ApplicationName);

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

    builder.Services.AddInvoiceFlowAuthentication(builder.Configuration);
    builder.Services.AddInvoiceFlowAuthorization();

    builder.AddApplicationServices();
    builder.AddInfrastructureServices();

    builder.Services
        .AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
        .AddDbContextCheck<IdentityDbContext>(name: "identity-database", tags: ["ready"]);

    var app = builder.Build();

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

    await app.ConfigureInfrastructureServices();

    await app.RunAsync();

}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync($"Application terminated unexpectedly: {ex}");

    throw;
}
