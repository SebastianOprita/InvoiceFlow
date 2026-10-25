using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record DeactivateRoleCommand(Guid TenantId, Guid RoleId) : IRequest<Result>;
