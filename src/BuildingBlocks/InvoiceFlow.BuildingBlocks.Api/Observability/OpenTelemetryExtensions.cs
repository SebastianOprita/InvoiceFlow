using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InvoiceFlow.BuildingBlocks.Api;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddInvoiceFlowObservability(
        this WebApplicationBuilder builder,
        string serviceName,
        params string[] additionalSources)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddSource("MassTransit")
                    .AddSource("InvoiceFlow.BuildingBlocks.Application")
                    .AddSource("Yarp.ReverseProxy")
                    .AddSource(additionalSources)
                    .AddOtlpExporter();
            })
            .WithLogging(logging =>
            {
                logging.AddOtlpExporter();
            });

        builder.Services.Configure<OpenTelemetryLoggerOptions>(options =>
        {
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.IncludeFormattedMessage = true;
        });

        return builder;
    }
}
