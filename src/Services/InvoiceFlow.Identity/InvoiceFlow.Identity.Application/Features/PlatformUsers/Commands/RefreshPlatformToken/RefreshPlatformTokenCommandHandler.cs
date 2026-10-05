using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using MediatR;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.Identity.Application;

public class RefreshPlatformTokenCommandHandler(
    IOptions<JwtSettings> jwtSettings,
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

        var existingToken = await platformRefreshTokensRepository.GetPlatformRefreshTokenAsync(RefreshTokenHash.Create(tokenHash), cancellationToken);

        if (existingToken is null)
            return Result<RefreshPlatformTokenResponse>.Failure(ApplicationErrors.RefreshTokenNotFound);

        if (!existingToken.IsActive(now))
            return Result<RefreshPlatformTokenResponse>.Failure(ApplicationErrors.RefreshTokenInvalid);

        var platformUser = await platformUsersRepository.FindUserByIdAsync(existingToken.UserId, cancellationToken);

        if (platformUser is null)
            return Result<RefreshPlatformTokenResponse>.Failure(ApplicationErrors.RefreshTokenInvalid);

        var accessToken = tokenService.GeneratePlatformUserAccessToken(
            platformUserId: platformUser.Id,
            platformEmail: platformUser.Email.Value,
            now);

        var rawNewRefreshToken = tokenService.GenerateRefreshToken();
        var newTokenHash = tokenService.CalculateTokenHash(rawNewRefreshToken);

        var newToken = existingToken.Rotate(
            RefreshTokenHash.Create(newTokenHash),
            now.AddDays(jwtSettings.Value.RefreshTokenDays),
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
            rawNewRefreshToken,
            platformUser.Id,
            platformUser.Email.Value));
    }
}
