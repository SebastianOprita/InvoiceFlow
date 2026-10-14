using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record RevokePermissionCommand(
    Guid TenantId,
    Guid RoleId,
    long Permission)
    : IRequest<Result<RoleDto>>;
