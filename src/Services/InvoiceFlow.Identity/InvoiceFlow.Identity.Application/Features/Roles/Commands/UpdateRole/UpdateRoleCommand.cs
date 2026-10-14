using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record UpdateRoleCommand(
    Guid TenantId,
    Guid RoleId,
    string Name,
    string? Description)
    : IRequest<Result<RoleDto>>;
