using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record AssignRoleCommand(
    Guid TenantId,
    Guid UserId,
    Guid RoleId)
    : IRequest<Result>;
