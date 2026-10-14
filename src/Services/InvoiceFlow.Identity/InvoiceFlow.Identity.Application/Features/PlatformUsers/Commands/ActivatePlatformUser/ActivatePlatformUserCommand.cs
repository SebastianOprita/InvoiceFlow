using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record ActivatePlatformUserCommand(
    Guid UserId)
    : IRequest<Result>;
