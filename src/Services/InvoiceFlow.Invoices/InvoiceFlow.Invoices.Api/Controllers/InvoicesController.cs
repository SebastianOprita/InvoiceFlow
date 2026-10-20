using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Invoices.Api;

[ApiController]
[Route("api/{tenantId:guid}/invoices")]
public class InvoicesController : ControllerBase
{
}
