using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class UpdatePermissionsCommandHandler(
    IUnitOfWork unitOfWork,
    IRolesRepository rolesRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdatePermissionsCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(UpdatePermissionsCommand cmd, CancellationToken cancellationToken)
    {
        var role = await rolesRepository.GetTrackedRoleByIdAsync(cmd.TenantId, cmd.RoleId, cancellationToken);
        if (role == null)
            return Result<RoleDto>.Failure(ApplicationErrors.RoleNotFound);

        role.SetPermissions(RolePermissions.Create(cmd.Permissions), dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<RoleDto>.Failure(result);

        return Result<RoleDto>.Success(role.ToDto());
    }
}
