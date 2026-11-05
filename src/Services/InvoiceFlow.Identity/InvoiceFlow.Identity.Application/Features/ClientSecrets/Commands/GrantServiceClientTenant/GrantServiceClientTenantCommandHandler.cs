using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class GrantServiceClientTenantCommandHandler(
    IServiceClientsRepository serviceClientsRepository,
    ITenantsRepository tenantsRepository,
    IUnitOfWork unitOfWork,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<GrantServiceClientTenantCommand, Result>
{
    public async Task<Result> Handle(
        GrantServiceClientTenantCommand cmd,
        CancellationToken cancellationToken)
    {
        var client =
            await serviceClientsRepository.GetTrackedServiceClientByIdAsync(
                cmd.ServiceClientId,
                cancellationToken);

        if (client is null)
            return Result.Failure(
                ApplicationErrors.ServiceClientNotFound);

        var tenant = await tenantsRepository.GetTenantByIdAsync(
            cmd.TenantId,
            cancellationToken);

        if (tenant is null)
            return Result.Failure(
                ApplicationErrors.TenantNotFound);

        client.GrantTenant(
            cmd.TenantId,
            dateTimeProvider.GetUtcNow());

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
