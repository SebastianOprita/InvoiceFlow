using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class LoginUserCommandHandler(
    IUnitOfWork unitOfWork,
    IRefreshTokensRepository refreshTokensRepository,
    ITenantsRepository tenantsRepository,
    IUsersRepository usersRepository,
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<LoginUserCommand, Result<LoginUserCommandResponse>>
{
    public async Task<Result<LoginUserCommandResponse>> Handle(LoginUserCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;

        var tenant = await tenantsRepository.GetTenantByIdAsync(cmd.TenantId, cancellationToken);
        if (tenant is null || !tenant.IsActive)
            return Result<LoginUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var existingUser = await usersRepository.GetUserByEmailAsync(cmd.TenantId, UserEmail.Create(cmd.Email), cancellationToken);
        if (existingUser is null || !existingUser.IsActive || !passwordHasher.VerifyPassword(cmd.Password, existingUser.PasswordHash.Value))
            return Result<LoginUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var userWithPermissions = await usersRepository.GetUserByIdWithPermissionsAsync(cmd.TenantId, existingUser.Id, cancellationToken);
        if (userWithPermissions is null || !userWithPermissions.IsActive)
            return Result<LoginUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

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
        var refershToken = new RefreshToken(
            cmd.TenantId,
            Guid.CreateVersion7(),
            userWithPermissions.Id,
            RefreshTokenHash.Create(newTokenHash),
            rawNewRefreshToken.ExpiresAt,
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
            rawNewRefreshToken.Token,
            userWithPermissions.Id,
            userWithPermissions.Email.Value));
    }
}
