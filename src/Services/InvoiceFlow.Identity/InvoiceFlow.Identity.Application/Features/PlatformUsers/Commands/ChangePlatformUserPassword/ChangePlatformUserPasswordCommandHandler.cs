using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class ChangePlatformUserPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository platformUsersRepository,
    IPasswordHasher passwordHasher,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ChangePlatformUserPasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePlatformUserPasswordCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;
        var platformUser = await platformUsersRepository.GetUserByIdAsync(cmd.UserId, cancellationToken);
        if (platformUser is null || !platformUser.IsActive)
            return Result.Failure(ApplicationErrors.UserNotFound);

        if (!passwordHasher.VerifyPassword(cmd.CurrentPassword, platformUser.PasswordHash.Value))
            return Result.Failure(ApplicationErrors.UserPasswordInvalid);

        platformUser.ChangePasswordHash(PasswordHash.Create(passwordHasher.HashPassword(cmd.NewPassword)), now);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
