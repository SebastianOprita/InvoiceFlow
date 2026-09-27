using InvoiceFlow.Customers.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Customers.Infrastructure;

public static class ConfigureServices
{
    public static WebApplicationBuilder RegisterInfrastructureServices(this WebApplicationBuilder builder)
    {
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

        return builder;
    }

    private static WebApplicationBuilder RegisterRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();

        return builder;
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
