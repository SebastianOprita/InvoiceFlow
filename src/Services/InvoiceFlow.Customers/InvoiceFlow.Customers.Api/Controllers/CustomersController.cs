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

    [HttpPost]
    [HasPermission(SystemPermission.CustomerCreate)]
    public async Task<IActionResult> Create([FromRoute] Guid tenantId, [FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(tenantId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { tenantId, customerId = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPatch("{customerId:guid}")]
    [HasPermission(SystemPermission.CustomerUpdate)]
    public async Task<IActionResult> Update([FromRoute] Guid tenantId, [FromRoute] Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(tenantId, customerId), ct);
        return result.ToActionResult();
    }

    [HttpPost("activate/{customerId:guid}")]
    [HasPermission(SystemPermission.CustomerDelete)]
    public async Task<IActionResult> Activate([FromRoute] Guid tenantId, [FromRoute] Guid customerId, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateCustomerCommand(tenantId, customerId), ct);
        return result.ToActionResult();
    }

    [HttpDelete("deactivate/{customerId:guid}")]
    [HasPermission(SystemPermission.CustomerDelete)]
    public async Task<IActionResult> Deactivate([FromRoute] Guid tenantId, [FromRoute] Guid customerId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateCustomerCommand(tenantId, customerId), ct);
        return result.ToActionResult();
    }
}
