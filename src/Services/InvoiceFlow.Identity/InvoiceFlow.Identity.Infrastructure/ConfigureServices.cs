using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Identity.Infrastructure;

public static class ConfigureServices
{
    public static WebApplicationBuilder RegisterInfrastructureServices(this WebApplicationBuilder builder)
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

        return builder;
    }

    private static WebApplicationBuilder RegisterRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
        builder.Services.AddScoped<IRolesRepository, RolesRepository>();
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<ITenantsRepository, TenantsRepository>();
        builder.Services.AddScoped<IPlatformRefreshTokensRepository, PlatformRefreshTokensRepository>();
        builder.Services.AddScoped<IPlatformUsersRepository, PlatformUsersRepository>();

        return builder;
    }

    public static async Task<WebApplication> ConfigureInfrastructureServices(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var datetimeProvider = scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>();

        db.Database.EnsureDeleted();
        db.Database.Migrate();

        await IdentityDbSeed.SeedAsync(db, datetimeProvider);

        return app;
    }
}
