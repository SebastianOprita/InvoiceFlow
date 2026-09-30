using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class RevokePermissionCommandHandler(
    IUnitOfWork unitOfWork,
    IRolesRepository rolesRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<RevokePermissionCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(RevokePermissionCommand cmd, CancellationToken cancellationToken)
    {
        var role = await rolesRepository.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId);
        if (role == null)
            return Result<RoleDto>.Failure(ApplicationErrors.RoleNotFound);

        role.RevokePermission(cmd.Permission, dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<RoleDto>.Failure(result);

        return Result<RoleDto>.Success(role.ToDto());
    }
}
