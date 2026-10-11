using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[Authorize]
[PlatformUserLogin]
[Route("api/platform/users")]
public class PlatformUsersController(IMediator mediator) : ApiController
{
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlatformUserByIdQuery(userId), ct);
        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlatformUsersQuery(), ct);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlatformUserRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { userId = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid userId, [FromBody] UpdatePlatformUserRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(userId), ct);
        return result.ToActionResult();
    }

    [HttpPost("activate/{userId:guid}")]
    public async Task<IActionResult> Activate([FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivatePlatformUserCommand(userId), ct);
        return result.ToActionResult();
    }

    [HttpDelete("deactivate/{userId:guid}")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivatePlatformUserCommand(userId), ct);
        return result.ToActionResult();
    }
}
