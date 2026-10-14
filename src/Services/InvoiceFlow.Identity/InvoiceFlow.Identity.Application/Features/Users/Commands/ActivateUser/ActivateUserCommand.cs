using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record ActivateUserCommand(
    Guid TenantId,
    Guid UserId)
    : IRequest<Result>;
