using InvoiceFlow.Customers.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Customers.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<JwtSettings>()
            .Bind(builder.Configuration.GetSection("JwtSettings"))
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.Secret),
                "JwtSettings:Secret is not configured.")
            .Validate(
                settings => settings.AccessTokenMinutes > 0 &&
                            settings.RefreshTokenDays > 0 &&
                            settings.ImpersonationTokenMinutes > 0,
                "JWT token lifetimes must be positive.")
            .ValidateOnStart();

        var connectionString = builder.Configuration.GetConnectionString("CustomersDb");

        builder.Services.AddDbContext<CustomersDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure();
                sql.MigrationsAssembly(typeof(CustomersDbContext).Assembly.FullName);
            });

            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.RegisterRepositories();
    }

    private static void RegisterRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
    }

    public static async Task<WebApplication> ConfigureInfrastructureServices(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        await CustomersDbSeed.SeedAsync(db);

        return app;
    }
}
