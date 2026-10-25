using System.ComponentModel.DataAnnotations;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Invoices.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Invoices.Api;

[ApiController]
[TenantScoped]
[TenantUserLogin]
[Route("api/{tenantId:guid}/invoices")]
public class InvoicesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{invoiceId:guid}")]
    [HasPermission(SystemPermissions.InvoiceRead)]
    public async Task<IActionResult> GetById(
        [FromRoute][Required] Guid tenantId,
        [FromRoute][Required] Guid invoiceId,
        CancellationToken cancellationToken)
    {
        return Ok();
    }

    [HttpPost]
    [HasPermission(SystemPermissions.InvoiceCreate)]
    public async Task<IActionResult> Create(
        [FromRoute][Required] Guid tenantId,
        [FromBody][Required] CreateInvoiceRequest request,
        [FromHeader(Name = "Idempotency-Key")][Required] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request.ToCommand(idempotencyKey, tenantId), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { tenantId, invoiceId = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPost("{invoiceId:guid}/Issue")]
    [HasPermission(SystemPermissions.CustomerStatusManage)]
    public async Task<IActionResult> Issue(
        [FromRoute][Required] Guid tenantId,
        [FromRoute][Required] Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new IssueInvoiceCommand(tenantId, invoiceId), cancellationToken);
        return result.ToActionResult();
    }
}
