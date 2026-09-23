using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.Common.Application;

public static class ConfigureServices
{
    public static WebApplicationBuilder RegisterCommonApplicationServices(this WebApplicationBuilder builder, params System.Reflection.Assembly[] additionalAssemblies)
    {
        var assembliesToRegister = new[] { typeof(ConfigureServices).Assembly }.Concat(additionalAssemblies).ToArray();

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assembliesToRegister);

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        builder.Services.AddValidatorsFromAssemblies(assembliesToRegister);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<ISystemDateTimeProvider, SystemDateTimeProvider>();

        return builder;
    }
}
