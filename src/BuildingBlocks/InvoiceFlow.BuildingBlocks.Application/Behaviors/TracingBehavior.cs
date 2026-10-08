using MediatR;
using System.Diagnostics;

namespace InvoiceFlow.BuildingBlocks.Application;

public sealed class TracingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);

        using var activity = ApplicationDiagnostics.Source
            .StartActivity(
                requestType.Name,
                ActivityKind.Internal);

        activity?.SetTag("mediatr.request.name", requestType.Name);
        activity?.SetTag("mediatr.request.type", requestType.FullName);

        try
        {
            var response = await next(cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return response;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            activity?.SetTag("mediatr.request.cancelled", true);

            throw;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error);

            activity?.AddEvent(
                new ActivityEvent(
                    "exception",
                    tags: new ActivityTagsCollection
                    {
                        { "exception.type", exception.GetType().FullName }
                    }));
            throw;
        }
    }
}
