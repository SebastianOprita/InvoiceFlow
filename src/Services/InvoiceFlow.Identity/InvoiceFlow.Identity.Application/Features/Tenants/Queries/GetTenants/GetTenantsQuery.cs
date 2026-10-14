using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public sealed record GetTenantsQuery
    : IRequest<Result<List<TenantDto>>>;
