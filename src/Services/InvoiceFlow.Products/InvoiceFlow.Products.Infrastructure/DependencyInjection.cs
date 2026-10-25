using InvoiceFlow.Products.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Products.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("ProductsDb");

        services.AddDbContext<ProductsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure();
                sql.MigrationsAssembly(
                    typeof(ProductsDbContext).Assembly.FullName);
            });

            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.RegisterRepositories();

        return services;
    }

    private static IServiceCollection RegisterRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IProductsRepository, ProductsRepository>();

        return services;
    }

    public static async Task ConfigureInfrastructureServicesAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        var dateTimeProvider = scope.ServiceProvider
            .GetRequiredService<BuildingBlocks.Application.ISystemDateTimeProvider>();
        await ProductsDbSeed.SeedAsync(db, dateTimeProvider, cancellationToken);
    }
}
