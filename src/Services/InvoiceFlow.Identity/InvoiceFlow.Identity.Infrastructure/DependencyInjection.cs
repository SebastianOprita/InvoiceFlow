using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("JwtSettings"))
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.Secret),
                "JwtSettings:Secret is not configured.")
            .Validate(
                settings => settings.AccessTokenMinutes > 0 &&
                            settings.RefreshTokenDays > 0 &&
                            settings.ImpersonationTokenMinutes > 0,
                "JWT token lifetimes must be positive.")
            .ValidateOnStart();

        var connectionString =
            configuration.GetConnectionString("IdentityDb");

        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure();
                sql.MigrationsAssembly(
                    typeof(IdentityDbContext).Assembly.FullName);
            });

            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });


        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<PasswordHasher<object>>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        services.RegisterRepositories();

        return services;
    }

    private static IServiceCollection RegisterRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();

        services.AddScoped<IRolesRepository, RolesRepository>();

        services.AddScoped<IUsersRepository, UsersRepository>();

        services.AddScoped<ITenantsRepository, TenantsRepository>();

        services.AddScoped<IPlatformRefreshTokensRepository, PlatformRefreshTokensRepository>();

        services.AddScoped<IPlatformUsersRepository, PlatformUsersRepository>();

        return services;
    }


    public static async Task ConfigureInfrastructureServicesAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<ISystemDateTimeProvider>();

        await db.Database.MigrateAsync(cancellationToken);

        await IdentityDbSeed.SeedAsync(db, dateTimeProvider, cancellationToken);
    }
}
