using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace InvoiceFlow.BuildingBlocks.Application;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ISystemDateTimeProvider _dateTimeProvider;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ISystemDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var traceId = Activity.Current?.TraceId.ToString();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId
        }))
        {
            _logger.LogInformation(
                "Handling {RequestName} with TraceId {TraceId} at {DateTimeUtc}",
                requestName,
                traceId,
                _dateTimeProvider.Now);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                return await next(cancellationToken);
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds} ms with TraceId {TraceId}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    traceId);

            }
        }
    }
}
