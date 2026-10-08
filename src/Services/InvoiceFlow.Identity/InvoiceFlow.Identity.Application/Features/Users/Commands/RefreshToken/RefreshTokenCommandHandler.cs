using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using MediatR;
using Microsoft.Extensions.Options;

namespace InvoiceFlow.Identity.Application;

public class RefreshTokenCommandHandler(
    IOptions<JwtSettings> jwtSettings,
    IUnitOfWork unitOfWork,
    IRefreshTokensRepository refreshTokensRepository,
    ITenantsRepository tenantsRepository,
    IUsersRepository usersRepository,
    ITokenService tokenService,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;

        var tenant = await tenantsRepository.GetTenantByIdAsync(cmd.TenantId, cancellationToken);
        if (tenant is null || !tenant.IsActive)
            return Result<RefreshTokenResponse>.Failure(ApplicationErrors.RefreshTokenInvalid);

        var tokenHash = tokenService.CalculateTokenHash(cmd.RefreshToken);

        var existingToken = await refreshTokensRepository.GetTrackedRefreshTokenAsync(cmd.TenantId, RefreshTokenHash.Create(tokenHash), cancellationToken);

        if (existingToken is null || !existingToken.IsActive(now))
            return Result<RefreshTokenResponse>.Failure(ApplicationErrors.RefreshTokenInvalid);

        var userWithPermissions = await usersRepository.GetUserByIdWithPermissionsAsync(cmd.TenantId, existingToken.UserId, cancellationToken);

        if (userWithPermissions is null || !userWithPermissions.IsActive)
            return Result<RefreshTokenResponse>.Failure(ApplicationErrors.UserNotFound);

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

        var newToken = existingToken.Rotate(
            RefreshTokenHash.Create(newTokenHash),
            now.AddDays(jwtSettings.Value.RefreshTokenDays),
            now,
            DeviceInfo.CreateOptional(cmd.DeviceInfo),
            IpAddress.CreateOptional(cmd.IpAddress));

        refreshTokensRepository.AddRefreshToken(newToken);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (result.IsFailure)
            return Result<RefreshTokenResponse>.Failure(result);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(
            accessToken.Token,
            accessToken.TokenType,
            accessToken.ExpiresAt,
            rawNewRefreshToken,
            userWithPermissions.Id,
            userWithPermissions.Email.Value));
    }
}
