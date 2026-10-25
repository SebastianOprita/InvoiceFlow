using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal sealed class ActivateRoleCommandHandler(
    IUnitOfWork unitOfWork,
    IRolesRepository rolesRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ActivateRoleCommand, Result>
{
    public async Task<Result> Handle(ActivateRoleCommand cmd, CancellationToken cancellationToken)
    {
        var role = await rolesRepository.GetTrackedRoleByIdAsync(cmd.TenantId, cmd.RoleId, cancellationToken);
        if (role is null)
            return Result.Failure(ApplicationErrors.RoleNotFound);

        if (role.IsActive)
            return Result.Success();

        role.Activate(dateTimeProvider.GetUtcNow());
        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
