using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Products.Application;

internal class GetProductByIdQueryHandler(IProductsRepository productsRepository) : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery qry, CancellationToken cancellationToken)
    {
        var product = await productsRepository.GetProductByIdAsync(qry.TenantId, qry.ProductId, cancellationToken);

        if (product is null)
            return Result<ProductDto>.Failure(ApplicationErrors.ProductNotFound);

        return Result<ProductDto>.Success(product.ToDto());
    }
}
