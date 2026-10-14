using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GetTenantByIdQuery(
    Guid TenantId)
    : IRequest<Result<TenantDto>>;
