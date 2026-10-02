using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Identity.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static void AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("IdentityDb");

        builder.Services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure();
                sql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
            });

            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddScoped<PasswordHasher<object>>();
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<ITokenService, TokenService>();

        builder.RegisterRepositories();
    }

    private static void RegisterRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
        builder.Services.AddScoped<IRolesRepository, RolesRepository>();
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<ITenantsRepository, TenantsRepository>();
        builder.Services.AddScoped<IPlatformRefreshTokensRepository, PlatformRefreshTokensRepository>();
        builder.Services.AddScoped<IPlatformUsersRepository, PlatformUsersRepository>();
    }

    public static async Task<WebApplication> ConfigureInfrastructureServices(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var datetimeProvider = scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        await IdentityDbSeed.SeedAsync(db, datetimeProvider);

        return app;
    }
}
