using FluentValidation;
using InvoiceFlow.BuildingBlocks.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Identity.Application;

public static class ConfigureServices
{
    public static WebApplicationBuilder RegisterApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(ConfigureServices).Assembly);

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        builder.Services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<ISystemDateTimeProvider, SystemDateTimeProvider>();

        return builder;
    }
}
