using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class RevokeServiceClientSecretCommandHandler(
    IServiceClientsRepository serviceClientRepository,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<RevokeServiceClientSecretCommand, Result>
{
    public async Task<Result> Handle(
        RevokeServiceClientSecretCommand cmd,
        CancellationToken cancellationToken)
    {
        var client = await serviceClientRepository.GetTrackedServiceClientByIdAsync(
            cmd.ServiceClientId,
            cancellationToken);

        if (client is null)
            return Result.Failure(
                ApplicationErrors.ServiceClientNotFound);

        var now = dateTimeProvider.GetUtcNow();

        client.RevokeSecret(
            cmd.SecretId,
            now);

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
