using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record DeactivatePlatformUserCommand(
    Guid UserId)
    : IRequest<Result>;
