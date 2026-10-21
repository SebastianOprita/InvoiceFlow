using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Invoices.Api;

[ApiController]
[Route("api/{tenantId:guid}/invoices")]
[AllowAnonymous]
public class InvoicesController : ControllerBase
{
}
