using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class GrantServiceClientPermissionsCommandHandler(
    IServiceClientsRepository serviceClientsRepository,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<GrantServiceClientPermissionsCommand, Result>
{
    public async Task<Result> Handle(
        GrantServiceClientPermissionsCommand cmd,
        CancellationToken cancellationToken)
    {
        var client =
            await serviceClientsRepository.GetTrackedServiceClientByIdAsync
            (cmd.ServiceClientId,
            cancellationToken);

        if (client is null)
            return Result.Failure(ApplicationErrors.ServiceClientNotFound);

        client.GrantPermission(cmd.Permission, dateTimeProvider.GetUtcNow());

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
