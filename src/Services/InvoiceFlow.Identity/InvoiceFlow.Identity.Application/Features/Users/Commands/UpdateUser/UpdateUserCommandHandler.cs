using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class UpdateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IUsersRepository usersRepository,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand cmd, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetUserByIdAsync(cmd.TenantId, cmd.UserId, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure(ApplicationErrors.UserNotFound);

        user.UpdateProfile(FirstName.Create(cmd.FirstName), LastName.Create(cmd.LastName), dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<UserDto>.Failure(result);

        return Result<UserDto>.Success(user.ToDto());
    }
}
