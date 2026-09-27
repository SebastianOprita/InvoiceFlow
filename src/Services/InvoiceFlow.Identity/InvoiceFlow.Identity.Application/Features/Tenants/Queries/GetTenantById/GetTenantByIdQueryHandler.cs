using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetTenantByIdQueryHandler(ITenantsRepository tenantsRepository) : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
{
    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery qry, CancellationToken cancellationToken)
    {
        var tenant = tenantsRepository.FindTenantById(qry.TenantId);

        if (tenant == null)
            return Result<TenantDto>.Failure(ApplicationErrors.TenantNotFound);

        return Result<TenantDto>.Success(tenant.ToDto());
    }
}
