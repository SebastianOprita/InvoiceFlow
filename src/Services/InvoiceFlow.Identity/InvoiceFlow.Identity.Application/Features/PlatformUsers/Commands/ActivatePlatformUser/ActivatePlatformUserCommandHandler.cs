using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class ActivatePlatformUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository usersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ActivatePlatformUserCommand, Result>
{
    public async Task<Result> Handle(ActivatePlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetUserByIdAsync(cmd.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(ApplicationErrors.UserNotFound);

        if (user.IsActive)
            return Result.Success();

        user.Activate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
