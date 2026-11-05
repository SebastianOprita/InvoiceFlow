using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class CreateServiceClientSecretCommandHandler(
    IServiceClientsRepository serviceClientRepository,
    IOrganizationsRepository organizationRepository,
    IServiceClientSecretService secretService,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateServiceClientSecretCommand,
        Result<CreateServiceClientSecretCommandResponse>>
{
    public async Task<Result<CreateServiceClientSecretCommandResponse>> Handle(
        CreateServiceClientSecretCommand cmd,
        CancellationToken cancellationToken)
    {
        var client = await serviceClientRepository.GetTrackedServiceClientByIdAsync(
            cmd.ServiceClientId,
            cancellationToken);

        if (client is null)
            return Result<CreateServiceClientSecretCommandResponse>.Failure(
                ApplicationErrors.ServiceClientNotFound);

        var organization = await organizationRepository.GetOrganizationByIdAsync(
            client.OrganizationId,
            cancellationToken);

        if (organization is null)
            return Result<CreateServiceClientSecretCommandResponse>.Failure(
                ApplicationErrors.OrganizationNotFound);

        if (!organization.IsActive)
            return Result<CreateServiceClientSecretCommandResponse>.Failure(
                ApplicationErrors.OrganizationInactive);

        if (!client.IsActive)
            return Result<CreateServiceClientSecretCommandResponse>.Failure(
                ApplicationErrors.ServiceClientInactive);

        var now = dateTimeProvider.GetUtcNow();

        var name = ServiceClientSecretName.Create(cmd.Name);

        var description =
            ServiceClientSecretDescription.CreateOptional(cmd.Description);

        var (secret, secretHash) = secretService.GenerateSecretAndHash();

        var clientSecret = client.AddSecret(
            secretId: Guid.CreateVersion7(),
            secretName: name,
            secretDescription: description,
            secretHash: secretHash,
            expiresAtUtc: cmd.ExpiresAtUtc,
            createdAtUtc: now);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return Result<CreateServiceClientSecretCommandResponse>.Failure(saveResult);

        return Result<CreateServiceClientSecretCommandResponse>.Success(
            new CreateServiceClientSecretCommandResponse(
                Id: clientSecret.Id,
                ServiceClientId: client.Id,
                Name: clientSecret.Name.Value,
                Secret: secret,
                CreatedAtUtc: clientSecret.CreatedAtUtc,
                ExpiresAtUtc: clientSecret.ExpiresAtUtc));
    }
}
