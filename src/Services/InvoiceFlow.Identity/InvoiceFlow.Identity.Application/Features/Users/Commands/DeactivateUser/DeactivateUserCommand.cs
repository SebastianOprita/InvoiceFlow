using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record DeactivateUserCommand(
    Guid TenantId,
    Guid UserId)
    : IRequest<Result>;
