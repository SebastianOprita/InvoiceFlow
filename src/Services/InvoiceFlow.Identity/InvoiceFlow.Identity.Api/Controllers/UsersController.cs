using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[TenantScoped]
[Route("api/{tenantId:guid}/users")]
public class UsersController(IMediator mediator) : ApiController
{
    [TenantUserLogin]
    [HasPermission(SystemPermissions.ManageUsers)]
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid tenantId, [FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserByIdQuery(tenantId, userId), ct);
        return result.ToActionResult();
    }

    [TenantUserLogin]
    [HasPermission(SystemPermissions.ManageUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUsersQuery(tenantId), ct);
        return result.ToActionResult();
    }

    [TenantUserLogin]
    [HasPermission(SystemPermissions.ManageUsers)]
    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] Guid tenantId, [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(tenantId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { tenantId, userId = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [TenantUserLogin]
    [HasPermission(SystemPermissions.ManageUsers)]
    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid tenantId, [FromRoute] Guid userId, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(tenantId, userId), ct);
        return result.ToActionResult();
    }

    [TenantUserLogin]
    [HasPermission(SystemPermissions.ManageUsers)]
    [HttpPost("activate/{userId:guid}")]
    public async Task<IActionResult> Activate([FromRoute] Guid tenantId, [FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateUserCommand(tenantId, userId), ct);
        return result.ToActionResult();
    }

    [TenantUserLogin]
    [HasPermission(SystemPermissions.ManageUsers)]
    [HttpDelete("deactivate/{userId:guid}")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid tenantId, [FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateUserCommand(tenantId, userId), ct);
        return result.ToActionResult();
    }

    [TenantUserLogin]
    [HasPermission(SystemPermissions.ManageUsers)]
    [HttpPost("{userId:guid}/roles")]
    public async Task<IActionResult> AssignRole([FromRoute] Guid tenantId, [FromRoute] Guid userId, [FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(request.ToCommand(tenantId, userId), ct);
        return result.ToActionResult();
    }

    [TenantUserLogin]
    [HasPermission(SystemPermissions.ManageUsers)]
    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RevokeRole([FromRoute] Guid tenantId, [FromRoute] Guid userId, [FromRoute] Guid roleId, CancellationToken ct)
    {
        var result = await mediator.Send(new RevokeUserRoleCommand(tenantId, userId, roleId), ct);
        return result.ToActionResult();
    }
}
