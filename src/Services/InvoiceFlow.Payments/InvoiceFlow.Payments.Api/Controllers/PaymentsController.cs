using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Payments.Api;

[ApiController]
[Route("api/{tenantId:guid}/payments")]
[AllowAnonymous]
public class PaymentsController : ControllerBase
{
}
