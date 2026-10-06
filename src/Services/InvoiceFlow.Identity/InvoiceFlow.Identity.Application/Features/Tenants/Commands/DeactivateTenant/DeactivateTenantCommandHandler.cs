using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class DeactivateTenantCommandHandler(
    IUnitOfWork unitOfWork,
    ITenantsRepository tenantsRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<DeactivateTenantCommand, Result>
{
    public async Task<Result> Handle(DeactivateTenantCommand cmd, CancellationToken cancellationToken)
    {
        var tenant = await tenantsRepository.GetTenantByIdAsync(cmd.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure(ApplicationErrors.TenantNotFound);

        if (!tenant.IsActive)
            return Result.Success();

        tenant.Deactivate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
