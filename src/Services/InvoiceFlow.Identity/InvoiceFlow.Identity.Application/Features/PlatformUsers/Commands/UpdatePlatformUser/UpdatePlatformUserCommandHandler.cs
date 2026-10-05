using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class UpdatePlatformUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository usersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdatePlatformUserCommand, Result<PlatformUserDto>>
{
    public async Task<Result<PlatformUserDto>> Handle(UpdatePlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetUserByIdAsync(cmd.UserId, cancellationToken);
        if (user is null)
            return Result<PlatformUserDto>.Failure(ApplicationErrors.UserNotFound);

        user.UpdateProfile(FirstName.Create(cmd.FirstName), LastName.Create(cmd.LastName), dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<PlatformUserDto>.Failure(result);

        return Result<PlatformUserDto>.Success(user.ToDto());
    }
}
