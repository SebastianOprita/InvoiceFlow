using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Payments.Domain;
using InvoiceFlow.Payments.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Respawn;
using Respawn.Graph;
using Xunit;

namespace InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;

public sealed class PaymentsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly JwtSettingsTestFixture _jwtSettingsFixture = new();
    private readonly DatabaseTestFixture _dbFixture = new();
    private Respawner _respawner = null!;

    public JwtValidationSettings JwtSettings => Services.GetRequiredService<IConfiguration>().GetSection("JwtSettings").Get<JwtValidationSettings>()
        ?? throw new InvalidOperationException("JwtSettings are not configured.");

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => config.AddConfiguration(_jwtSettingsFixture.Configuration));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:PaymentsDb"] = _dbFixture.ConnectionString()
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<PaymentsDbContext>>();
            services.AddDbContext<PaymentsDbContext>(options => options.UseSqlServer(_dbFixture.ConnectionString()));
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _dbFixture.InitializeAsync();
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);
        await using var connection = new SqlConnection(_dbFixture.ConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(_dbFixture.ConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await _respawner.ResetAsync(connection);
    }

    public async Task SeedPaymentAsync(Payment payment)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        db.Payments.Add(payment);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public async Task SeedInvoicePaymentBalanceAsync(InvoicePaymentBalance balance)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        db.InvoicePaymentBalances.Add(balance);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public new async ValueTask DisposeAsync()
    {
        await _dbFixture.DisposeAsync();
        await base.DisposeAsync();
    }
}
