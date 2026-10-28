using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Invoices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InvoicesDb");

        services.AddDbContext<InvoicesDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(2),
                    errorNumbersToAdd: [1205, 1222]);
                sql.MigrationsAssembly(
                    typeof(InvoicesDbContext).Assembly.FullName);
            });

            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IRequestHasher, RequestHasher>();

        services.RegisterRepositories();
        services.RegisterHttpClients();

        return services;
    }

    private static IServiceCollection RegisterRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceNumberRepository, InvoiceNumberRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

        return services;
    }

    private static IServiceCollection RegisterHttpClients(
        this IServiceCollection services)
    {
        services.AddScoped<ICustomersService, CustomersService>();
        services.AddScoped<IProductsService, ProductsService>();

        return services;
    }

    public static async Task ConfigureInfrastructureServicesAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<InvoicesDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        var dateTimeProvider = scope.ServiceProvider
            .GetRequiredService<BuildingBlocks.Application.ISystemDateTimeProvider>();

        await InvoicesDbSeed.SeedAsync(db, dateTimeProvider, cancellationToken);
    }
}
