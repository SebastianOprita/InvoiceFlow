using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GrantPermissionCommand(
    Guid TenantId,
    Guid RoleId,
    long Permission)
    : IRequest<Result<RoleDto>>;
