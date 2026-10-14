using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record UpdatePermissionsCommand(
    Guid TenantId,
    Guid RoleId,
    long Permissions)
    : IRequest<Result<RoleDto>>;
