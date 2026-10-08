using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class DeactivatePlatformUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository platformUsersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<DeactivatePlatformUserCommand, Result>
{
    public async Task<Result> Handle(DeactivatePlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var platformUser = await platformUsersRepository.GetTrackedUserByIdAsync(cmd.UserId, cancellationToken);
        if (platformUser is null)
            return Result.Failure(ApplicationErrors.UserNotFound);

        if (!platformUser.IsActive)
            return Result.Success();

        platformUser.Deactivate(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
