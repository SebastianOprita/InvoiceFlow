using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class GenerateServiceClientTokenCommandHandler(
    IServiceClientsRepository serviceClientsRepository,
    IOrganizationsRepository organizationsRepository,
    ITenantsRepository tenantsRepository,
    IServiceClientSecretService secretService,
    ITokenService tokenService,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<GenerateServiceClientTokenCommand, Result<GenerateServiceClientTokenCommandResponse>>
{
    public async Task<Result<GenerateServiceClientTokenCommandResponse>> Handle(
        GenerateServiceClientTokenCommand cmd,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.GetUtcNow();

        var clientId = ServiceClientId.Create(cmd.ClientId);

        var client =
            await serviceClientsRepository.GetServiceClientByClientIdAsync(
                clientId,
                cancellationToken);

        if (client is null || !client.IsActive)
            return Result<GenerateServiceClientTokenCommandResponse>.Failure(
                ApplicationErrors.InvalidServiceClientCredentials);

        var organization = await organizationsRepository.GetOrganizationByIdAsync(
            client.OrganizationId,
            cancellationToken);

        if (organization is null || !organization.IsActive)
            return Result<GenerateServiceClientTokenCommandResponse>.Failure(
                ApplicationErrors.InvalidServiceClientCredentials);

        var secretValid = client.Secrets.Any(secret =>
            secret.IsActive(now) &&
            secretService.Verify(
                cmd.ClientSecret,
                secret.SecretHash));

        if (!secretValid)
            return Result<GenerateServiceClientTokenCommandResponse>.Failure(
                ApplicationErrors.InvalidServiceClientCredentials);

        var tenant = await tenantsRepository.GetTenantByIdAsync(
            cmd.TenantId,
            cancellationToken);

        if (tenant is null || !tenant.IsActive)
            return Result<GenerateServiceClientTokenCommandResponse>.Failure(
                ApplicationErrors.ServiceClientAccessDenied);

        if (!client.CanAccessTenant(cmd.TenantId))
            return Result<GenerateServiceClientTokenCommandResponse>.Failure(
                ApplicationErrors.ServiceClientAccessDenied);

        var token = tokenService.GenerateServiceClientToken(
            client,
            cmd.TenantId,
            now);

        return Result<GenerateServiceClientTokenCommandResponse>.Success(
            new GenerateServiceClientTokenCommandResponse
            (
                AccessToken: token.Token,
                TokenType: token.TokenType,
                AccessTokenExpiresAt: token.ExpiresAt
            ));
    }
}
