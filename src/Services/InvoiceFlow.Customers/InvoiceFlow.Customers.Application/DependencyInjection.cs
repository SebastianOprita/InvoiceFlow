using FluentValidation;
using InvoiceFlow.BuildingBlocks.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace InvoiceFlow.Customers.Application;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly);

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        builder.Services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<ISystemDateTimeProvider, SystemDateTimeProvider>();

        return builder;
    }
}
