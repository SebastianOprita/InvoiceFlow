using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace InvoiceFlow.BuildingBlocks.Api;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var errorId = Guid.NewGuid().ToString("N");
        var traceId = Activity.Current?.TraceId.ToString();

        logger.LogError(
            exception,
            "Unhandled exception. ErrorId: {ErrorId}, ExceptionType: {ExceptionType}, TraceId: {TraceId}, Method: {Method}, Path: {Path}",
            errorId,
            exception.GetType().Name,
            traceId,
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                message = "An unexpected error occurred.",
                errorId
            },
            cancellationToken);

        return true;
    }
}
