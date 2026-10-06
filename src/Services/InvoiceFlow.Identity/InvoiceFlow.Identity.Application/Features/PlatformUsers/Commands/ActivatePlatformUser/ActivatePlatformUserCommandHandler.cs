using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class ActivatePlatformUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository platformUsersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ActivatePlatformUserCommand, Result>
{
    public async Task<Result> Handle(ActivatePlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var platformUser = await platformUsersRepository.GetUserByIdAsync(cmd.UserId, cancellationToken);
        if (platformUser is null)
            return Result.Failure(ApplicationErrors.UserNotFound);

        if (platformUser.IsActive)
            return Result.Success();

        platformUser.Activate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
