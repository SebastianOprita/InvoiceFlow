using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class LogoutPlatformUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformRefreshTokensRepository platformRefreshTokensRepository,
    ITokenService tokenService,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<LogoutPlatformUserCommand, Result>
{
    public async Task<Result> Handle(LogoutPlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.CalculateTokenHash(cmd.RefreshToken);

        var existingToken = await platformRefreshTokensRepository.GetTrackedPlatformRefreshTokenAsync(RefreshTokenHash.Create(tokenHash), cancellationToken);
        if (existingToken is null || existingToken.IsRevoked)
            return Result.Success();

        existingToken.Revoke(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (result.IsFailure)
            return Result.Failure(result);

        return Result.Success();
    }
}
