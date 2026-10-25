using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[TenantScoped]
[TenantUserLogin]
[Route("api/{tenantId:guid}/roles")]
public class RolesController(IMediator mediator) : ApiController
{
    [HttpGet("{roleId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid tenantId, [FromRoute] Guid roleId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRoleByIdQuery(tenantId, roleId), ct);
        return result.ToActionResult();
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromRoute] Guid tenantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRolesQuery(tenantId), ct);
        return result.ToActionResult();
    }

    [HasPermission(SystemPermissions.RoleCreate)]
    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] Guid tenantId, [FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateRoleCommand(tenantId, request.Name, request.Description, (long)request.Permissions), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { tenantId, roleId = result.Value!.Id }, result.Value)
            : result.ToActionResult();
    }

    [HasPermission(SystemPermissions.RoleUpdate)]
    [HttpPut("{roleId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid tenantId, [FromRoute] Guid roleId, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateRoleCommand(tenantId, roleId, request.Name, request.Description), ct);
        return result.ToActionResult();
    }

    [HasPermission(SystemPermissions.RoleStatusManage)]
    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid tenantId, [FromRoute] Guid roleId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteRoleCommand(tenantId, roleId), ct);
        return result.ToActionResult();
    }

    [HasPermission(SystemPermissions.RolePermissionsUpdate)]
    [HttpPost("{roleId:guid}/permissions/grant")]
    public async Task<IActionResult> GrantPermission([FromRoute] Guid tenantId, [FromRoute] Guid roleId, [FromBody] UpdatePermissionsRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new GrantPermissionCommand(tenantId, roleId, (long)request.Permissions), ct);
        return result.ToActionResult();
    }

    [HasPermission(SystemPermissions.RolePermissionsUpdate)]
    [HttpPost("{roleId:guid}/permissions/revoke")]
    public async Task<IActionResult> RevokePermission([FromRoute] Guid tenantId, [FromRoute] Guid roleId, [FromBody] UpdatePermissionsRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new RevokePermissionCommand(tenantId, roleId, (long)request.Permissions), ct);
        return result.ToActionResult();
    }
}
