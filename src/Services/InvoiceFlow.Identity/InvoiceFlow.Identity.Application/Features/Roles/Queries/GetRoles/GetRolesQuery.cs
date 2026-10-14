using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GetRolesQuery(
    Guid TenantId)
    : IRequest<Result<List<RoleDto>>>;
