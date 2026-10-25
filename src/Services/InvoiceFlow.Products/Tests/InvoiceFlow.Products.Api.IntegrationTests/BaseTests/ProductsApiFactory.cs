using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Products.Domain;
using InvoiceFlow.Products.Infrastructure;
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

namespace InvoiceFlow.Products.Api.IntegrationTests.BaseTests;

public sealed class ProductsApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly JwtSettingsTestFixture _jwtSettingsFixture;
    private readonly DatabaseTestFixture _dbFixture;
    private Respawner _respawner = null!;
    public JwtValidationSettings JwtSettings
    {
        get
        {
            var configuration = Services
                .GetRequiredService<IConfiguration>();

            return configuration
                .GetSection("JwtSettings")
                .Get<JwtValidationSettings>()
                ?? throw new InvalidOperationException(
                    "JwtSettings are not configured.");
        }
    }

    public ProductsApiFactory()
    {
        _jwtSettingsFixture = new JwtSettingsTestFixture();
        _dbFixture = new DatabaseTestFixture();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddConfiguration(_jwtSettingsFixture.Configuration);
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ProductsDbContext>>();

                services.AddDbContext<ProductsDbContext>(options =>
                {
                    options.UseSqlServer(_dbFixture.ConnectionString());
                });
            });
    }

    public async ValueTask InitializeAsync()
    {
        await _dbFixture.InitializeAsync();

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);

        await using var connection = new SqlConnection(_dbFixture.ConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
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

    internal async Task<Guid> InsertProductAsync(Guid tenantId, string productCode)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>();
        var product = CreateProduct(tenantId, productCode, dateTimeProvider.GetUtcNow());
        db.Products.Add(product);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return product.Id;

    }

    private static Product CreateProduct(Guid tenantId, string productCode, DateTimeOffset dateTime)
    {
        return Product.Create(
            tenantId,
            Guid.CreateVersion7(),
            ProductCode.Create(productCode),
            ProductName.Create("product-name"),
            ProductDescription.Create("product-description"),
            UnitOfMeasure.Piece,
            Money.Create(123, CurrencyCode.Create("RON")),
            Percentage.Create(10),
            dateTime);
    }
}
