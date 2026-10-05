using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GetRolesQuery(
    Guid TenantId)
    : IRequest<Result<List<RoleDto>>>;
