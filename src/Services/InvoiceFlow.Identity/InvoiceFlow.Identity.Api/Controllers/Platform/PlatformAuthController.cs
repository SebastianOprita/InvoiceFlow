using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api.Controllers;

[ApiController]
[Route("api/platform/auth")]
public class PlatformAuthController(IMediator mediator) : ApiController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginPlatformUserCommand(
            request.Email,
            request.Password,
            DeviceInfo,
            IpAddress), ct);

        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshPlatformTokenCommand(
            request.RefreshToken,
            DeviceInfo,
            IpAddress), ct);

        return result.ToActionResult();
    }

    [PlatformUserLogin]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var _))
            return Unauthorized();

        var result = await mediator.Send(new LogoutPlatformUserCommand(request.RefreshToken), ct);

        return result.ToActionResult();
    }

    [PlatformUserLogin]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized();

        var result = await mediator.Send(new ChangePlatformUserPasswordCommand(
            currentUserId,
            request.CurrentPassword,
            request.NewPassword), ct);

        return result.ToActionResult();
    }

    [PlatformUserLogin]
    [HttpPost("impersonate-user")]
    public async Task<IActionResult> ImpersonateUser([FromBody] ImpersonateUserRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized();

        var result = await mediator.Send(new ImpersonateUserCommand(
            currentUserId,
            request.TargetUserTenantId,
            request.TargetUserEmail,
            request.Reason,
            DeviceInfo,
            IpAddress), ct);

        return result.ToActionResult();
    }
}
