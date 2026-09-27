using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Authorization.Permissions;
using InvoiceFlow.Identity.Domain;
using MediatR;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.Identity.Application;

public class LoginUserCommandHandler(
    IOptions<JwtSettings> jwtSettings,
    IUnitOfWork unitOfWork,
    IRefreshTokensRepository refreshTokensRepository,
    IUsersRepository usersRepository,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<LoginUserCommand, Result<LoginUserCommandResponse>>
{
    public async Task<Result<LoginUserCommandResponse>> Handle(LoginUserCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;

        var existingUser = usersRepository.FindUserByEmail(cmd.TenantId, UserEmail.Create(cmd.Email));
        if (existingUser is null || !passwordHasher.VerifyPassword(cmd.Password, existingUser.PasswordHash.Value))
            return Result<LoginUserCommandResponse>.Failure(ApplicationErrors.LoginUserNotFound);

        var userWithPermissions = usersRepository.FindUserByIdWithPermissions(cmd.TenantId, existingUser.Id);

        if (userWithPermissions is null)
            return Result<LoginUserCommandResponse>.Failure(ApplicationErrors.LoginUserNotFound);

        var aggregatedPermissions = userWithPermissions.UserRoles.Select(ur => ur.Role.Permissions)
            .Aggregate(SystemPermission.None, (current, rolePermissions) => current | rolePermissions.Value);

        var accessToken = tokenService.GenerateUserAccessToken(
            userId: userWithPermissions.Id,
            tenantId: userWithPermissions.TenantId,
            email: userWithPermissions.Email.Value,
            permissions: aggregatedPermissions,
            now);

        var rawNewRefreshToken = tokenService.GenerateRefreshToken();
        var newTokenHash = tokenService.CalculateTokenHash(rawNewRefreshToken);
        var refershToken = new RefreshToken(
            cmd.TenantId,
            Guid.CreateVersion7(),
            userWithPermissions.Id,
            RefreshTokenHash.Create(newTokenHash),
            now.AddDays(jwtSettings.Value.RefreshTokenDays),
            now,
            DeviceInfo.CreateOptional(cmd.DeviceInfo),
            IpAddress.CreateOptional(cmd.IpAddress));

        refreshTokensRepository.AddRefreshToken(refershToken);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (result.IsFailure)
            return Result<LoginUserCommandResponse>.Failure(result);

        return Result<LoginUserCommandResponse>.Success(new LoginUserCommandResponse(
            accessToken.Token,
            accessToken.TokenType,
            accessToken.ExpiresAt,
            rawNewRefreshToken,
            userWithPermissions.Id,
            userWithPermissions.Email.Value));
    }
}
