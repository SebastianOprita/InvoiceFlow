using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class DeleteRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IRolesRepository rolesRepository)
    : IRequestHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> Handle(DeleteRoleCommand cmd, CancellationToken cancellationToken)
    {
        var role = await rolesRepository.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId, cancellationToken);
        if (role == null)
            return Result<RoleDto>.Failure(ApplicationErrors.RoleNotFound);

        role.RemoveFromAllUsers();
        rolesRepository.RemoveRole(role);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}
