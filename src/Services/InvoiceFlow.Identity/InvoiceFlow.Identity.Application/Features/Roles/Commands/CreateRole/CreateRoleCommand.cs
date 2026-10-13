using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record CreateRoleCommand(
    Guid TenantId,
    string Name,
    string? Description,
    long Permissions)
    : IRequest<Result<RoleDto>>;
