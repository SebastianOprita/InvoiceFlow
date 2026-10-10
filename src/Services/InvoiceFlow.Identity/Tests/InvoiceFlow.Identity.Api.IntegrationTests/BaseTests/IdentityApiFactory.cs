using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using InvoiceFlow.Identity.Infrastructure;
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

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

public sealed class IdentityApiFactory
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

    public IdentityApiFactory()
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
                services.RemoveAll<DbContextOptions<IdentityDbContext>>();

                services.AddDbContext<IdentityDbContext>(options =>
                {
                    options.UseSqlServer(_dbFixture.ConnectionString());
                });
            });
    }

    public async ValueTask InitializeAsync()
    {
        await _dbFixture.InitializeAsync();

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await db.Database.MigrateAsync(TestContext.Current.CancellationToken);

        await using var connection = new SqlConnection(_dbFixture.ConnectionString());
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        });

        await InitializeTestDataAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(_dbFixture.ConnectionString());

        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await _respawner.ResetAsync(connection);

        await InitializeTestDataAsync();
    }


    public new async ValueTask DisposeAsync()
    {
        await _dbFixture.DisposeAsync();
        await base.DisposeAsync();
    }

    public Tenant Tenant { get; private set; } = null!;
    public Tenant InactiveTenant { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public PlatformUser PlatformUser { get; private set; } = null!;

    private async Task InitializeTestDataAsync()
    {
        using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>();

        var tenant = Tenant.Create
        (
            Guid.CreateVersion7(),
            TenantName.Create("TestTenant"),
            TenantSlug.Create("test-tenant"),
            dateTimeProvider.Now
        );
        db.Tenants.Add(tenant);
        var inactiveTenant = Tenant.Create
        (
            Guid.CreateVersion7(),
            TenantName.Create("InactiveTestTenant"),
            TenantSlug.Create("inactive-test-tenant"),
            dateTimeProvider.Now
        );
        db.Tenants.Add(inactiveTenant);
        var user = CreateUser(tenant.Id, "user@email.com", dateTimeProvider.Now, TestConstants.Password);
        db.Users.Add(user);
        var platformUser = CreatePlatformUser("platformuser@email.com", dateTimeProvider.Now, TestConstants.Password);
        db.PlatformUsers.Add(platformUser);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Tenant = tenant;
        InactiveTenant = inactiveTenant;
        User = user;
        PlatformUser = platformUser;
    }

    private static User CreateUser(Guid tenantId, string email, DateTime dateTime, string password = "Password", Guid? userId = null)
    {
        var id = userId ?? Guid.CreateVersion7();
        return User.Create
        (
            tenantId,
            id,
            UserEmail.Create(email),
            PasswordHash.Create(password),
            FirstName.Create($"FirstName"),
            LastName.Create($"LastName"),
            dateTime
        );
    }

    private static PlatformUser CreatePlatformUser(string email, DateTime dateTime, string password = "Password", Guid? userId = null)
    {
        var id = userId ?? Guid.CreateVersion7();
        return PlatformUser.Create
        (
            id,
            UserEmail.Create(email),
            PasswordHash.Create(password),
            FirstName.Create($"FirstName"),
            LastName.Create($"LastName"),
            dateTime
        );
    }

    internal async Task<Guid> InsertUserAsync(Guid tenantId, string email, string password = "Password", Guid? userId = null)
    {
        using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>();

        var user = CreateUser(tenantId, email, dateTimeProvider.Now, password, userId);
        db.Users.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user.Id;
    }

    internal Guid InsertTenant(string name, string slug)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>();

        var tenant = Tenant.Create
        (
            Guid.CreateVersion7(),
            TenantName.Create(name),
            TenantSlug.Create(slug),
            dateTimeProvider.Now
        );
        db.Tenants.Add(tenant);
        db.SaveChanges();
        return tenant.Id;
    }
}
