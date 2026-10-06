using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class CreateTenantCommandHandler(
    IUnitOfWork unitOfWork,
    ITenantsRepository tenantsRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateTenantCommand, Result<TenantDto>>
{
    public async Task<Result<TenantDto>> Handle(CreateTenantCommand cmd, CancellationToken cancellationToken)
    {
        var alreadyExists = await tenantsRepository.ExistsBySlugAsync(TenantSlug.Create(cmd.Slug), cancellationToken);

        if (alreadyExists)
            return Result<TenantDto>.Failure(ApplicationErrors.TenantSlugAlreadyExists);

        var tenant = Tenant.Create(
            Guid.CreateVersion7(),
            TenantName.Create(cmd.Name),
            TenantSlug.Create(cmd.Slug),
            dateTimeProvider.Now);

        tenantsRepository.AddTenant(tenant);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<TenantDto>.Failure(result);

        return Result<TenantDto>.Success(tenant.ToDto());
    }
}
