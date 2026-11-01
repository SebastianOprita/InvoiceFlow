using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class RevokeServiceClientTenantCommandHandler(
    IServiceClientsRepository serviceClientsRepository,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<RevokeServiceClientTenantCommand, Result>
{
    public async Task<Result> Handle(
        RevokeServiceClientTenantCommand cmd,
        CancellationToken cancellationToken)
    {
        var client =
            await serviceClientsRepository.GetTrackedServiceClientByIdAsync(
                cmd.ServiceClientId,
                cancellationToken);

        if (client is null)
            return Result.Failure(
                ApplicationErrors.ServiceClientNotFound);

        client.RevokeTenant(
            cmd.TenantId,
            dateTimeProvider.GetUtcNow());

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
