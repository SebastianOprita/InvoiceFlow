using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal class GrantPermissionCommandHandler(
    IUnitOfWork unitOfWork,
    IRolesRepository rolesRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<GrantPermissionCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(GrantPermissionCommand cmd, CancellationToken cancellationToken)
    {
        var role = await rolesRepository.GetTrackedRoleByIdAsync(cmd.TenantId, cmd.RoleId, cancellationToken);
        if (role == null)
            return Result<RoleDto>.Failure(ApplicationErrors.RoleNotFound);

        role.GrantPermission(cmd.Permission, dateTimeProvider.GetUtcNow());
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<RoleDto>.Failure(result);

        return Result<RoleDto>.Success(role.ToDto());
    }
}
