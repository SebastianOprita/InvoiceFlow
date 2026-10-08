using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using MediatR;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.Identity.Application;

public class LoginPlatformUserCommandHandler(
    IOptions<JwtSettings> jwtSettings,
    IUnitOfWork unitOfWork,
    IPlatformRefreshTokensRepository platformRefreshTokensRepository,
    IPlatformUsersRepository platformUsersRepository,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<LoginPlatformUserCommand, Result<LoginPlatformUserCommandResponse>>
{
    public async Task<Result<LoginPlatformUserCommandResponse>> Handle(LoginPlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;

        var existingPlatfromUser = await platformUsersRepository.GetUserByEmailAsync(UserEmail.Create(cmd.Email), cancellationToken);
        if (existingPlatfromUser is null || !existingPlatfromUser.IsActive || !passwordHasher.VerifyPassword(cmd.Password, existingPlatfromUser.PasswordHash.Value))
            return Result<LoginPlatformUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var accessToken = tokenService.GeneratePlatformUserAccessToken(
            platformUserId: existingPlatfromUser.Id,
            platformEmail: existingPlatfromUser.Email.Value,
            now);

        var rawNewRefreshToken = tokenService.GenerateRefreshToken();
        var newTokenHash = tokenService.CalculateTokenHash(rawNewRefreshToken);
        var refershToken = PlatformRefreshToken.Create(
            Guid.CreateVersion7(),
            existingPlatfromUser.Id,
            RefreshTokenHash.Create(newTokenHash),
            now.AddDays(jwtSettings.Value.RefreshTokenDays),
            now,
            DeviceInfo.CreateOptional(cmd.DeviceInfo),
            IpAddress.CreateOptional(cmd.IpAddress));

        platformRefreshTokensRepository.AddPlatformRefreshToken(refershToken);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (result.IsFailure)
            return Result<LoginPlatformUserCommandResponse>.Failure(result);

        return Result<LoginPlatformUserCommandResponse>.Success(new LoginPlatformUserCommandResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            rawNewRefreshToken,
            existingPlatfromUser.Id,
            existingPlatfromUser.Email.Value));
    }
}
