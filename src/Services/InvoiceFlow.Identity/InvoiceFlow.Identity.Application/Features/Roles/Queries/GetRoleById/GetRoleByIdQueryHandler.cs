using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetRoleByIdQueryHandler(IRolesRepository rolesRepository) : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery qry, CancellationToken cmd)
    {
        var role = rolesRepository.FindRoleById(qry.TenantId, qry.RoleId);

        if (role == null)
            return Result<RoleDto>.Failure(ApplicationErrors.RoleNotFound);

        return Result<RoleDto>.Success(role.ToDto());
    }
}
