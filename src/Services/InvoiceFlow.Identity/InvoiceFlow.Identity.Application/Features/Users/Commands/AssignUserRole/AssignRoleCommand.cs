using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record AssignRoleCommand(
    Guid TenantId,
    Guid UserId,
    Guid RoleId)
    : IRequest<Result>;
