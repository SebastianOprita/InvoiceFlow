using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Products.Application;

internal class GetProductsQueryHandler(IProductsRepository productsRepository) : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery qry, CancellationToken cancellationToken)
    {
        var products = await productsRepository.GetAllProductsAsync(qry.TenantId, cancellationToken);
        var productsDto = products.Select(p => p.ToDto()).ToList();

        return Result<List<ProductDto>>.Success(productsDto);
    }
}
