using System.ComponentModel.DataAnnotations;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Payments.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Payments.Api;

[ApiController]
[TenantScoped]
[TenantUserLogin]
[Route("api/{tenantId:guid}/payments")]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(SystemPermissions.PaymentRead)]
    public async Task<IActionResult> GetAll(
        [FromRoute][Required] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPaymentsQuery(tenantId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{paymentId:guid}")]
    [HasPermission(SystemPermissions.PaymentRead)]
    public async Task<IActionResult> GetById(
        [FromRoute][Required] Guid tenantId,
        [FromRoute][Required] Guid paymentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPaymentByIdQuery(tenantId, paymentId), cancellationToken);
        return result.ToActionResult();
    }
}
