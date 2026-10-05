using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record CreateRoleCommand(
    Guid TenantId,
    string Name,
    string? Description,
    SystemPermission Permissions)
    : IRequest<Result<RoleDto>>;
