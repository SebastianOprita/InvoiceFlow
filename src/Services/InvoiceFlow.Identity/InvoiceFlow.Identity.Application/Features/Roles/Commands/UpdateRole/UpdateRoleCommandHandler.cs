using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class UpdateRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IRolesRepository rolesRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand cmd, CancellationToken cancellationToken)
    {
        var role = await rolesRepository.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId);

        if (role == null)
            return Result<RoleDto>.Failure(ApplicationErrors.RoleNotFound);

        if (role.Name.Value != cmd.Name)
        {
            var roleWithNameExists = await rolesRepository.ExistsByNameAsync(cmd.TenantId, RoleName.Create(cmd.Name));
            if (roleWithNameExists)
                return Result<RoleDto>.Failure(ApplicationErrors.RoleNameAlreadyExists);
        }

        role.UpdateDetails(RoleName.Create(cmd.Name), dateTimeProvider.Now, RoleDescription.CreateOptional(cmd.Description));
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<RoleDto>.Failure(result);

        return Result<RoleDto>.Success(role.ToDto());
    }
}
