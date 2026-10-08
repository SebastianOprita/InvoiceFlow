using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class ActivateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IUsersRepository usersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ActivateUserCommand, Result>
{
    public async Task<Result> Handle(ActivateUserCommand cmd, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(ApplicationErrors.UserNotFound);

        if (user.IsActive)
            return Result.Success();

        user.Activate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
