using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

public record LoginRequest(string Email, string Password);

[ApiController]
[Authorize]
[TenantScoped]
[Route("api/{tenantId:guid}/[controller]")]
public class AuthController(IMediator mediator) : ApiController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromRoute] Guid tenantId, [FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginUserCommand(
            tenantId,
            request.Email,
            request.Password,
            DeviceInfo,
            IpAddress), cancellationToken);

        return result.ToActionResult();
    }
}
