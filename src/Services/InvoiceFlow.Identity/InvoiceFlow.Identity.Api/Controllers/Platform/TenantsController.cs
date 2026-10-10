using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[Authorize]
[PlatformUserLogin]
[Route("api/platform/tenants")]
public class TenantsController(IMediator mediator) : ApiController
{
    [HttpGet("{tenantId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(tenantId), ct);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetTenantsQuery(), ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { tenantId = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPut("{tenantId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid tenantId, [FromBody] UpdateTenantRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(tenantId), ct);
        return result.ToActionResult();
    }

    [HttpPost("activate/{tenantId:guid}")]
    public async Task<IActionResult> Activate([FromRoute] Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateTenantCommand(tenantId), ct);
        return result.ToActionResult();
    }

    [HttpDelete("deactivate/{tenantId:guid}")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateTenantCommand(tenantId), ct);
        return result.ToActionResult();
    }
}
