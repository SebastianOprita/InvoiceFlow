using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GetTenantByIdQuery(
    Guid TenantId)
    : IRequest<Result<TenantDto>>;
