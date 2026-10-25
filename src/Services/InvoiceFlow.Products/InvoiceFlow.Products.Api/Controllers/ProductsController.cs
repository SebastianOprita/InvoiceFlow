using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Products.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Products.Api.Controllers;

[ApiController]
[TenantScoped]
[TenantUserLogin]
[Route("api/{tenantId:guid}/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(SystemPermissions.ProductRead)]
    public async Task<IActionResult> GetAll([FromRoute] Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductsQuery(tenantId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{productId:guid}")]
    [HasPermission(SystemPermissions.ProductRead)]
    public async Task<IActionResult> GetById([FromRoute] Guid tenantId, [FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProductByIdQuery(tenantId, productId), cancellationToken);
        return result.ToActionResult();
    }
}
