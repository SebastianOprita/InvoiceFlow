using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class UpdateTenantCommandHandler(
    IUnitOfWork unitOfWork,
    ITenantsRepository tenantsRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateTenantCommand, Result<TenantDto>>
{
    public async Task<Result<TenantDto>> Handle(UpdateTenantCommand cmd, CancellationToken cancellationToken)
    {
        var tenant = await tenantsRepository.GetTrackedTenantByIdAsync(cmd.TenantId, cancellationToken);
        if (tenant is null)
            return Result<TenantDto>.Failure(ApplicationErrors.TenantNotFound);

        tenant.UpdateName(TenantName.Create(cmd.Name), dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<TenantDto>.Failure(result);

        return Result<TenantDto>.Success(tenant.ToDto());
    }
}
