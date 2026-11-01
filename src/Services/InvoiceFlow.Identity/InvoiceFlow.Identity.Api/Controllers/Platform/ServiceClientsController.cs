using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[PlatformUserLogin]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("api/platform/service-clients")]
public class ServiceClientsController(IMediator mediator) : ApiController
{
    [HttpGet("{serviceClientId:guid}/secrets")]
    public async Task<IActionResult> GetSecrets(
        Guid serviceClientId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetServiceClientSecretsQuery(serviceClientId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{serviceClientId:guid}/secrets")]
    public async Task<IActionResult> CreateSecret(
       Guid serviceClientId,
       [FromBody] CreateServiceClientSecretRequest request,
       CancellationToken cancellationToken)
    {
        var command = new CreateServiceClientSecretCommand(
            ServiceClientId: serviceClientId,
            Name: request.Name,
            Description: request.Description,
            ExpiresAtUtc: request.ExpiresAtUtc);

        var result = await mediator.Send(
            command,
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{serviceClientId:guid}/secrets/{secretId:guid}")]
    public async Task<IActionResult> RevokeSecret(
        Guid serviceClientId,
        Guid secretId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RevokeServiceClientSecretCommand(
                serviceClientId,
                secretId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{serviceClientId:guid}/permissions")]
    public async Task<IActionResult> GrantPermissions(
        Guid serviceClientId,
        [FromBody] GrantServiceClientPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GrantServiceClientPermissionsCommand(
                serviceClientId,
                request.Permission),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{serviceClientId:guid}/permissions/{permission:long}")]
    public async Task<IActionResult> RevokePermission(
        Guid serviceClientId,
        long permission,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RevokeServiceClientPermissionsCommand(
                serviceClientId,
                permission),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{serviceClientId:guid}/tenants")]
    public async Task<IActionResult> GrantTenant(
        Guid serviceClientId,
        [FromBody] GrantServiceClientTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GrantServiceClientTenantCommand(
                serviceClientId,
                request.TenantId),
            cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{serviceClientId:guid}/tenants/{tenantId:guid}")]
    public async Task<IActionResult> RevokeTenant(
        Guid serviceClientId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RevokeServiceClientTenantCommand(
                serviceClientId,
                tenantId),
            cancellationToken);

        return result.ToActionResult();
    }
}
