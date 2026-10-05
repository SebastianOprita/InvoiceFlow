using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record ActivatePlatformUserCommand(
    Guid UserId)
    : IRequest<Result>;
