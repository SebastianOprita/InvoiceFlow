using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetTenantsQueryHandler(ITenantsRepository tenantsRepository) : IRequestHandler<GetTenantsQuery, Result<List<TenantDto>>>
{
    public async Task<Result<List<TenantDto>>> Handle(GetTenantsQuery qry, CancellationToken cancellationToken)
    {
        var tenants = await tenantsRepository.GetAllTenantsAsync();
        var tenantDtos = tenants
            .Select(t => t.ToDto())
            .ToList();

        return Result<List<TenantDto>>.Success(tenantDtos);
    }
}
