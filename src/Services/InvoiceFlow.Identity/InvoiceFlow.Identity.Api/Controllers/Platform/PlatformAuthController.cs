using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api.Controllers;

[ApiController]
[Route("api/platform/auth")]
public class PlatformAuthController(IMediator mediator) : ApiController
{
}
