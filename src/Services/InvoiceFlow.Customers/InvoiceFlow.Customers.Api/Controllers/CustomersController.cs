using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Customers.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Application;

namespace InvoiceFlow.Customers.Api;

[ApiController]
[Authorize]
[TenantScoped]
[TenantUserLogin]
[Route("api/{tenantId:guid}/[controller]")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(SystemPermission.CustomerView)]
    public async Task<IActionResult> GetAll([FromRoute] Guid tenantId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCustomersQuery(tenantId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{customerId:guid}")]
    [HasPermission(SystemPermission.CustomerView)]
    public async Task<IActionResult> GetById([FromRoute] Guid tenantId, [FromRoute] Guid customerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(tenantId, customerId), cancellationToken);
        return result.ToActionResult();
    }
}
