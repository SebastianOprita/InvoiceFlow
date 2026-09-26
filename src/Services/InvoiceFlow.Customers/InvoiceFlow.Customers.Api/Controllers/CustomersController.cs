using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.Customers.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvoiceFlow.BuildingBlocks.Authorization;

namespace InvoiceFlow.Customers.Api;

[ApiController]
[Authorize]
[TenantScoped]
[Route("api/{tenantId:guid}/[controller]")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(SystemPermission.CustomerView)]
    public async Task<IActionResult> GetAll([FromRoute] Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomersQuery(tenantId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{customerId:guid}")]
    [HasPermission(SystemPermission.CustomerView)]
    public async Task<IActionResult> GetById([FromRoute] Guid tenantId, [FromRoute] Guid customerId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(tenantId, customerId), ct);
        return result.ToActionResult();
    }
}
