using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Products.Application;

public sealed record GetProductByIdQuery(
    Guid TenantId,
    Guid ProductId)
    : IRequest<Result<ProductDto>>;
