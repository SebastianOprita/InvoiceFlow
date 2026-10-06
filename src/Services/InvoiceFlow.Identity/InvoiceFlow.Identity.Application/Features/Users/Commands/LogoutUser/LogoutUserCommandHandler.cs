using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class LogoutUserCommandHandler(
    IUnitOfWork unitOfWork,
    IRefreshTokensRepository refreshTokensRepository,
    ITokenService tokenService,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<LogoutUserCommand, Result>
{
    public async Task<Result> Handle(LogoutUserCommand cmd, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.CalculateTokenHash(cmd.RefreshToken);

        var existingToken = await refreshTokensRepository.GetRefreshTokenAsync(cmd.TenantId, RefreshTokenHash.Create(tokenHash), cancellationToken);
        if (existingToken is null || existingToken.IsRevoked)
            return Result.Success();

        existingToken.Revoke(dateTimeProvider.Now);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (result.IsFailure)
            return Result.Failure(result);

        return Result.Success();
    }
}