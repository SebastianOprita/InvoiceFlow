using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record CreateRoleCommand(
    Guid TenantId,
    string Name,
    string? Description,
    long Permissions)
    : IRequest<Result<RoleDto>>;
