using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetRolesQueryHandler(IRolesRepository rolesRepository) : IRequestHandler<GetRolesQuery, Result<List<RoleDto>>>
{
    public async Task<Result<List<RoleDto>>> Handle(GetRolesQuery qry, CancellationToken cancellationToken)
    {
        var roles = await rolesRepository.GetAllRolesAsync(qry.TenantId, cancellationToken);
        var rolesDto = roles
            .Select(r => r.ToDto())
            .ToList();

        return Result<List<RoleDto>>.Success(rolesDto);
    }
}
