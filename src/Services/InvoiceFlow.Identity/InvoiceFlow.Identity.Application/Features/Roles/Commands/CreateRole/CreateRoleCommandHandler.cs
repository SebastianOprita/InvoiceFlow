using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class CreateRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IRolesRepository rolesRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(CreateRoleCommand cmd, CancellationToken cancellationToken)
    {
        var alreadyExists = await rolesRepository.ExistsByNameAsync(cmd.TenantId, RoleName.Create(cmd.Name), cancellationToken);

        if (alreadyExists)
            return Result<RoleDto>.Failure(ApplicationErrors.RoleNameAlreadyExists);

        var role = Role.Create(
            cmd.TenantId,
            Guid.CreateVersion7(),
            RoleName.Create(cmd.Name),
            dateTimeProvider.Now,
            RoleDescription.CreateOptional(cmd.Description),
            RolePermissions.Create(cmd.Permissions));
        rolesRepository.AddRole(role);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<RoleDto>.Failure(result);

        return Result<RoleDto>.Success(role.ToDto());
    }
}
