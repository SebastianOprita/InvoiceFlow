using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class UpdatePlatformUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository platformUsersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdatePlatformUserCommand, Result<PlatformUserDto>>
{
    public async Task<Result<PlatformUserDto>> Handle(UpdatePlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var platformUser = await platformUsersRepository.GetTrackedUserByIdAsync(cmd.UserId, cancellationToken);
        if (platformUser is null)
            return Result<PlatformUserDto>.Failure(ApplicationErrors.UserNotFound);

        platformUser.UpdateProfile(FirstName.Create(cmd.FirstName), LastName.Create(cmd.LastName), dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<PlatformUserDto>.Failure(result);

        return Result<PlatformUserDto>.Success(platformUser.ToDto());
    }
}
