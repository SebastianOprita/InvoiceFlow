using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class ChangeUserPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IUsersRepository usersRepository,
    IRefreshTokensRepository refreshTokensRepository,
    IPasswordHasher passwordHasher,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ChangeUserPasswordCommand, Result>
{
    public async Task<Result> Handle(ChangeUserPasswordCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;
        var user = await usersRepository.GetUserByIdAsync(cmd.TenantId, cmd.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            return Result.Failure(ApplicationErrors.UserNotFound);

        if (!passwordHasher.VerifyPassword(cmd.CurrentPassword, user.PasswordHash.Value))
            return Result.Failure(ApplicationErrors.UserPasswordInvalid);

        user.ChangePasswordHash(PasswordHash.Create(passwordHasher.HashPassword(cmd.NewPassword)), now);
        await refreshTokensRepository.RevokeAccessForUserAsync(cmd.TenantId, cmd.UserId, now, cancellationToken);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }
}
