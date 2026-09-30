using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record UpdateRoleCommand(
    Guid TenantId,
    Guid RoleId,
    string Name,
    string? Description)
    : IRequest<Result<RoleDto>>, ITenantScopedRequest;
