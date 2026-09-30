using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record RevokePermissionCommand(
    Guid TenantId,
    Guid RoleId,
    SystemPermission Permission)
    : IRequest<Result<RoleDto>>, ITenantScopedRequest;
