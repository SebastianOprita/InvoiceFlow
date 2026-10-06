using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class RevokeUserRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IUsersRepository usersRepository,
    IRolesRepository rolesRepository)
    : IRequestHandler<RevokeUserRoleCommand, Result>
{
    public async Task<Result> Handle(RevokeUserRoleCommand cmd, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetUserByIdAsync(cmd.TenantId, cmd.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(ApplicationErrors.UserNotFound);

        var role = await rolesRepository.FindRoleByIdAsync(cmd.TenantId, cmd.RoleId, cancellationToken);
        if (role is null)
            return Result.Failure(ApplicationErrors.RoleNotFound);

        user.RevokeRole(cmd.RoleId);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
