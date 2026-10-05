using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class ChangePlatformUserPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository usersRepository,
    IPasswordHasher passwordHasher,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ChangePlatformUserPasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePlatformUserPasswordCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;
        var user = await usersRepository.GetUserByIdAsync(cmd.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            return Result.Failure(ApplicationErrors.UserNotFound);

        if (!passwordHasher.VerifyPassword(cmd.CurrentPassword, user.PasswordHash.Value))
            return Result.Failure(ApplicationErrors.UserPasswordInvalid);

        user.ChangePasswordHash(PasswordHash.Create(passwordHasher.HashPassword(cmd.NewPassword)), now);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
