using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class DeactivatePlatformUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository usersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<DeactivatePlatformUserCommand, Result>
{
    public async Task<Result> Handle(DeactivatePlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetUserByIdAsync(cmd.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(ApplicationErrors.UserNotFound);

        if (!user.IsActive)
            return Result.Success();

        user.Deactivate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
