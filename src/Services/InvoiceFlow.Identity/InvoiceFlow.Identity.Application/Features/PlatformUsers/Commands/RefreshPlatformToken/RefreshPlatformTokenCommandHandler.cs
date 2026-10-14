using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal class RefreshPlatformTokenCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformRefreshTokensRepository platformRefreshTokensRepository,
    IPlatformUsersRepository platformUsersRepository,
    ITokenService tokenService,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<RefreshPlatformTokenCommand, Result<RefreshPlatformTokenResponse>>
{
    public async Task<Result<RefreshPlatformTokenResponse>> Handle(RefreshPlatformTokenCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;
        var tokenHash = tokenService.CalculateTokenHash(cmd.RefreshToken);

        var existingToken = await platformRefreshTokensRepository.GetTrackedPlatformRefreshTokenAsync(RefreshTokenHash.Create(tokenHash), cancellationToken);

        if (existingToken is null)
            return Result<RefreshPlatformTokenResponse>.Failure(ApplicationErrors.RefreshTokenNotFound);

        if (!existingToken.IsActive(now))
            return Result<RefreshPlatformTokenResponse>.Failure(ApplicationErrors.RefreshTokenInvalid);

        var platformUser = await platformUsersRepository.GetUserByIdAsync(existingToken.UserId, cancellationToken);

        if (platformUser is null || !platformUser.IsActive)
            return Result<RefreshPlatformTokenResponse>.Failure(ApplicationErrors.RefreshTokenInvalid);

        var accessToken = tokenService.GeneratePlatformUserAccessToken(
            platformUser.Id,
            platformUser.Email.Value,
            now);

        var rawNewRefreshToken = tokenService.GenerateRefreshToken(now);
        var newTokenHash = tokenService.CalculateTokenHash(rawNewRefreshToken.Token);

        var newToken = existingToken.Rotate(
            RefreshTokenHash.Create(newTokenHash),
            rawNewRefreshToken.ExpiresAt,
            now,
            DeviceInfo.CreateOptional(cmd.DeviceInfo),
            IpAddress.CreateOptional(cmd.IpAddress));

        platformRefreshTokensRepository.AddPlatformRefreshToken(newToken);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (result.IsFailure)
            return Result<RefreshPlatformTokenResponse>.Failure(result);

        return Result<RefreshPlatformTokenResponse>.Success(new RefreshPlatformTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            rawNewRefreshToken.Token,
            platformUser.Id,
            platformUser.Email.Value));
    }
}
