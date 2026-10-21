using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Payments.Api;

[ApiController]
[Route("api/{tenantId:guid}/payments")]
public class PaymentsController : ControllerBase
{
}
