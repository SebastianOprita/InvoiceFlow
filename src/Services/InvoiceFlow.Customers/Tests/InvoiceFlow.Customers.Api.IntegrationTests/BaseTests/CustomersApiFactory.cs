using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Customers.Domain;
using InvoiceFlow.Customers.Infrastructure;
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

namespace InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;

public sealed class CustomersApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly JwtSettingsTestFixture _jwtSettingsFixture;
    private readonly DatabaseTestFixture _dbFixture;
    private Respawner _respawner = null!;
    public JwtSettings JwtSettings
    {
        get
        {
            var configuration = Services
                .GetRequiredService<IConfiguration>();

            return configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    "JwtSettings are not configured.");
        }
    }

    public CustomersApiFactory()
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
                services.RemoveAll<DbContextOptions<CustomersDbContext>>();

                services.AddDbContext<CustomersDbContext>(options =>
                {
                    options.UseSqlServer(_dbFixture.ConnectionString());
                });
            });
    }

    public async ValueTask InitializeAsync()
    {
        await _dbFixture.InitializeAsync();

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();

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


    internal async Task<Guid> InsertCustomerAsync(Guid tenantId, string customerCode, string registrationNumber, string? taxNumber)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();

        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>();

        var customer = CreateCustomer(tenantId, customerCode, registrationNumber, taxNumber, dateTimeProvider.Now);
        db.Customers.Add(customer);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return customer.Id;
    }

    private static Customer CreateCustomer(Guid tenantId, string customerCode, string registrationNumber, string? taxNumber, DateTime dateTime)
    {
        var id = Guid.CreateVersion7();
        return Customer.Create
        (
            id,
            tenantId,
            CustomerCode.Create(customerCode),
            CustomerName.Create($"Customer-{id:N}"),
            CustomerEmail.Create($"customer-{id:N}@email.com"),
            PhoneNumber.Create("00767057577"),
            TaxNumber.CreateOptional(taxNumber),
            RegistrationNumber.Create(registrationNumber),
            AddressLine1.Create("Strada Mihai Eminescu"),
            AddressLine2.CreateOptional("Numarul 2"),
            City.Create("Bucharest"),
            State.CreateOptional("Bucharest"),
            Country.Create("Romania"),
            PostalCode.Create("010000"),
            CurrencyCode.Create("RON"),
            CreditLimit.Create(1200),
            PaymentTermDays.Create(30),
            dateTime
        );
    }
}
