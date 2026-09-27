using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace InvoiceFlow.BuildingBlocks.Application;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISystemDateTimeProvider _dateTimeProvider;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor,
        ISystemDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = GetCorrelationId();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            _logger.LogInformation(
                "Handling {RequestName} with CorrelationId {CorrelationId} at {DateTimeUtc}",
                requestName,
                correlationId,
                _dateTimeProvider.Now);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next(cancellationToken);
                return response;
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds} ms with CorrelationId {CorrelationId}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
        }
    }

    private string GetCorrelationId()
    {
        var context = _httpContextAccessor.HttpContext;

        if (context == null)
            return Guid.CreateVersion7().ToString();

        // Try custom header first
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            return correlationId.ToString();

        // Fallback to built-in trace identifier
        return context.TraceIdentifier;
    }
}
