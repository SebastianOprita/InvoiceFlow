using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GetRoleByIdQuery(
    Guid TenantId,
    Guid RoleId)
    : IRequest<Result<RoleDto>>;
