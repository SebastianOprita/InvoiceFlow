using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public record GetTenantsQuery
    : IRequest<Result<List<TenantDto>>>;
