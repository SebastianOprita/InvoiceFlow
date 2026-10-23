using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

internal class RefreshTokenCommandHandler(
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
        var now = dateTimeProvider.GetUtcNow();

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
            .Aggregate(RolePermissions.None.Value, (current, rolePermissions) => current | rolePermissions.Value);

        var accessToken = tokenService.GenerateUserAccessToken(
            userId: userWithPermissions.Id,
            tenantId: userWithPermissions.TenantId,
            email: userWithPermissions.Email.Value,
            permissions: aggregatedPermissions,
            now);

        var rawNewRefreshToken = tokenService.GenerateRefreshToken(now);
        var newTokenHash = tokenService.CalculateTokenHash(rawNewRefreshToken.Token);

        var newToken = existingToken.Rotate(
            RefreshTokenHash.Create(newTokenHash),
            rawNewRefreshToken.ExpiresAt,
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
            rawNewRefreshToken.Token,
            userWithPermissions.Id,
            userWithPermissions.Email.Value));
    }
}
