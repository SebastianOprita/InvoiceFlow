using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Products.Application;

public sealed record GetProductsQuery(
    Guid TenantId)
    : IRequest<Result<List<ProductDto>>>;
