using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace InvoiceFlow.BuildingBlocks.Api;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddInvoiceFlowTracing(
        this IServiceCollection services,
        string serviceName)
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName);
            })
            .WithTracing(tracing =>
            {
                tracing
                    // Incoming HTTP
                    .AddAspNetCoreInstrumentation()

                    // Outgoing HttpClient
                    .AddHttpClientInstrumentation()

                    // EF Core -> Microsoft.Data.SqlClient
                    .AddSqlClientInstrumentation()

                    // MassTransit -> RabbitMQ
                    .AddSource("MassTransit")

                    // Export to OTLP collector / Aspire
                    .AddOtlpExporter();
            });

        return services;
    }
}
