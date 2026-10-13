using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record UpdatePermissionsCommand(
    Guid TenantId,
    Guid RoleId,
    long Permissions)
    : IRequest<Result<RoleDto>>;
