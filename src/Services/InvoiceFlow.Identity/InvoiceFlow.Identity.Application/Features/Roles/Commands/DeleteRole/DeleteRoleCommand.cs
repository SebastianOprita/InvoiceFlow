using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record DeleteRoleCommand(
    Guid TenantId,
    Guid RoleId)
    : IRequest<Result>;
