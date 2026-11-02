using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api.Controllers.Platform;

[ApiController]
[Route("connect")]
[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None)]
public sealed class TokenController(IMediator mediator)
    : ApiController
{
    [AllowAnonymous]
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token(
       [FromForm] ClientCredentialsTokenRequest request,
       CancellationToken cancellationToken)
    {

    }
}
