using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record DeleteRoleCommand(
    Guid TenantId,
    Guid RoleId)
    : IRequest<Result>;
