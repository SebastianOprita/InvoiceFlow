using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[TenantScoped]
[Route("api/{tenantId:guid}/auth")]
public class AuthController(IMediator mediator) : ApiController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromRoute] Guid tenantId, [FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginUserCommand(
            tenantId,
            request.Email,
            request.Password,
            DeviceInfo,
            IpAddress), ct);

        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromRoute] Guid tenantId, [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(
            tenantId,
            request.RefreshToken,
            DeviceInfo,
            IpAddress), ct);

        return result.ToActionResult();
    }

    [Authorize]
    [TenantUserLogin]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromRoute] Guid tenantId, [FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized();

        var result = await mediator.Send(new LogoutUserCommand(
            tenantId,
            request.RefreshToken), ct);

        return result.ToActionResult();
    }

    [Authorize]
    [TenantUserLogin]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromRoute] Guid tenantId, [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized();

        var result = await mediator.Send(new ChangeUserPasswordCommand(
            tenantId,
            currentUserId,
            request.CurrentPassword,
            request.NewPassword), ct);

        return result.ToActionResult();
    }
}
