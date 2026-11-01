using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class RevokeServiceClientPermissionsCommandHandler(
    IServiceClientsRepository serviceClientsRepository,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<RevokeServiceClientPermissionsCommand, Result>
{
    public async Task<Result> Handle(
        RevokeServiceClientPermissionsCommand cmd,
        CancellationToken cancellationToken)
    {
        var client =
            await serviceClientsRepository.GetTrackedServiceClientByIdAsync
            (cmd.ServiceClientId,
            cancellationToken);

        if (client is null)
            return Result.Failure(ApplicationErrors.ServiceClientNotFound);

        client.RevokePermission(cmd.Permission, dateTimeProvider.GetUtcNow());

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
