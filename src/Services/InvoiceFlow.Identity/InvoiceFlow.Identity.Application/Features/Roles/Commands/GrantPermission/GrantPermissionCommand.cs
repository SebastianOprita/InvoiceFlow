using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GrantPermissionCommand(
    Guid TenantId,
    Guid RoleId,
    long Permission)
    : IRequest<Result<RoleDto>>;
