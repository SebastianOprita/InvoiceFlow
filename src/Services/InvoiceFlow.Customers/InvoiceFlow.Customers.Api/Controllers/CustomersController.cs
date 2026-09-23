using InvoiceFlow.Common.Api;
using InvoiceFlow.Customers.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Customers.Api;

[ApiController]
[Route("api/{tenantId:guid}/[controller]")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomersQuery(tenantId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid tenantId, [FromRoute] Guid customerId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(tenantId, customerId), ct);
        return result.ToActionResult();
    }
}
