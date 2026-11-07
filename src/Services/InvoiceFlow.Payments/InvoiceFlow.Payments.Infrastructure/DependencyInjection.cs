using InvoiceFlow.Payments.Application;
using InvoiceFlow.Payments.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentsDb");

        services.AddDbContext<PaymentsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(2),
                    errorNumbersToAdd: [1205, 1222]);
                sql.MigrationsAssembly(
                    typeof(PaymentsDbContext).Assembly.FullName);
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
        services.AddScoped<IPaymentsRepository, PaymentsRepository>();
        services.AddScoped<IIdempotencyRecordsRepository, IdempotencyRecordsRepository>();

        return services;
    }

    private static IServiceCollection RegisterHttpClients(
        this IServiceCollection services)
    {
        //services.AddScoped<IInvoicesService, InvoicesService>();

        return services;
    }

    public static async Task ConfigureInfrastructureServicesAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        var dateTimeProvider = scope.ServiceProvider
            .GetRequiredService<BuildingBlocks.Application.ISystemDateTimeProvider>();

        await PaymentsDbSeed.SeedAsync(db, dateTimeProvider, cancellationToken);
    }
}
