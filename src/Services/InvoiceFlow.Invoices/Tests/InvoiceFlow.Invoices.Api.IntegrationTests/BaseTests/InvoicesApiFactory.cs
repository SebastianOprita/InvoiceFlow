using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Invoices.Infrastructure;
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

namespace InvoiceFlow.Invoices.Api.IntegrationTests.BaseTests;

public sealed class InvoicesApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly JwtSettingsTestFixture _jwtSettingsFixture;
    private readonly DatabaseTestFixture _dbFixture;
    private Respawner _respawner = null!;

    public JwtValidationSettings JwtSettings
    {
        get
        {
            var configuration = Services.GetRequiredService<IConfiguration>();
            return configuration.GetSection("JwtSettings").Get<JwtValidationSettings>()
                ?? throw new InvalidOperationException("JwtSettings are not configured.");
        }
    }

    public InvoicesApiFactory()
    {
        _jwtSettingsFixture = new JwtSettingsTestFixture();
        _dbFixture = new DatabaseTestFixture();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
            config.AddConfiguration(_jwtSettingsFixture.Configuration));

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<InvoicesDbContext>>();
            services.AddDbContext<InvoicesDbContext>(options =>
                options.UseSqlServer(_dbFixture.ConnectionString()));
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _dbFixture.InitializeAsync();

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InvoicesDbContext>();
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

    public new async ValueTask DisposeAsync()
    {
        await _dbFixture.DisposeAsync();
        await base.DisposeAsync();
    }
}
