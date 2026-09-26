using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetTenantsQueryHandler(ITenantsRepository tenantsRepository) : IRequestHandler<GetTenantsQuery, Result<List<TenantDto>>>
{
    public async Task<Result<List<TenantDto>>> Handle(GetTenantsQuery qry, CancellationToken ct)
    {
        var tenants = tenantsRepository.FindAllTenants()
            .Select(t => t.ToDto())
            .ToList();

        return Result<List<TenantDto>>.Success(tenants);
    }
}
