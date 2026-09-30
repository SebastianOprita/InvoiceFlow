using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record UpdatePermissionsCommand(
    Guid TenantId,
    Guid RoleId,
    SystemPermission Permissions)
    : IRequest<Result<RoleDto>>, ITenantScopedRequest;
