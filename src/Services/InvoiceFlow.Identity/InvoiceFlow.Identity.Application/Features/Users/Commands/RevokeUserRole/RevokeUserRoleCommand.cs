using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record RevokeUserRoleCommand(
    Guid TenantId,
    Guid UserId,
    Guid RoleId)
    : IRequest<Result>;
