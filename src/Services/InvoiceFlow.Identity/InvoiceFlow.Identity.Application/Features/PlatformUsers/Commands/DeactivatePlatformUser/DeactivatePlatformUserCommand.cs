using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record DeactivatePlatformUserCommand(
    Guid UserId)
    : IRequest<Result>;
