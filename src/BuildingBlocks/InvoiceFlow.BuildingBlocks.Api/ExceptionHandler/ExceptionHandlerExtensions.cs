using Microsoft.Extensions.DependencyInjection;

namespace InvoiceFlow.BuildingBlocks.Api;

public static class ExceptionHandlerExtensions
{
    public static IServiceCollection AddInvoiceFlowExceptionHandling(
        this IServiceCollection services)
    {
        services.AddProblemDetails();

        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}

