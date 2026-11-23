using System.ComponentModel.DataAnnotations;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
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

    [HttpPost]
    [HasPermission(SystemPermissions.PaymentCreate)]
    public async Task<IActionResult> Create(
        [FromRoute][Required] Guid tenantId,
        [FromBody][Required] CreatePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")][Required] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(InvoiceFlowClaimTypes.UserId)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(request.ToCommand(idempotencyKey, tenantId, userId), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { tenantId, paymentId = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPost("{paymentId:guid}/Post")]
    [HasPermission(SystemPermissions.PaymentPost)]
    public async Task<IActionResult> Post(
        [FromRoute][Required] Guid tenantId,
        [FromRoute][Required] Guid paymentId,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(InvoiceFlowClaimTypes.UserId)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(new PostPaymentCommand(tenantId, paymentId, userId), cancellationToken);
        return result.ToActionResult();
    }
}
