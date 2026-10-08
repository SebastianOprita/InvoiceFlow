using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class AssignUserRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IUsersRepository usersRepository,
    IRolesRepository rolesRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<AssignRoleCommand, Result>
{
    public async Task<Result> Handle(AssignRoleCommand cmd, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(ApplicationErrors.UserNotFound);

        var role = await rolesRepository.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId, cancellationToken);
        if (role is null)
            return Result.Failure(ApplicationErrors.RoleNotFound);

        user.AssignRole(cmd.RoleId, dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
