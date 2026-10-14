using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record RevokePermissionCommand(
    Guid TenantId,
    Guid RoleId,
    long Permission)
    : IRequest<Result<RoleDto>>;
