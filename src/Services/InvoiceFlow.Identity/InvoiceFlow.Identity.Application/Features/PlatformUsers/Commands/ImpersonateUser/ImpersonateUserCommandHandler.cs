using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class ImpersonateUserCommandHandler(
    ITenantsRepository tenantsRepository,
    IPlatformUsersRepository platformUsersRepository,
    IUsersRepository usersRepository,
    ITokenService tokenService,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ImpersonateUserCommand, Result<ImpersonateUserCommandResponse>>
{
    public async Task<Result<ImpersonateUserCommandResponse>> Handle(ImpersonateUserCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;

        var platformUser = await platformUsersRepository.GetUserByIdAsync(cmd.ActorUserId, cancellationToken);
        if (platformUser is null || !platformUser.IsActive)
            return Result<ImpersonateUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var tenant = await tenantsRepository.GetTenantByIdAsync(cmd.TargetUserTenantId,cancellationToken);
        if (tenant is null || !tenant.IsActive)
            return Result<ImpersonateUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var existingUser = await usersRepository.GetUserByEmailAsync(cmd.TargetUserTenantId, UserEmail.Create(cmd.TargetUserEmail), cancellationToken);
        if (existingUser is null)
            return Result<ImpersonateUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var userWithPermissions = await usersRepository.GetUserByIdWithPermissionsAsync(existingUser.TenantId, existingUser.Id, cancellationToken);
        if (userWithPermissions is null)
            return Result<ImpersonateUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var aggregatedPermissions = userWithPermissions.UserRoles.Select(ur => ur.Role.Permissions)
            .Aggregate(SystemPermission.None, (current, rolePermissions) => current | rolePermissions.Value);

        var impersonationToken = tokenService.GenerateImpersonationToken(
            platformUserId: platformUser.Id,
            platformEmail: platformUser.Email.Value,
            targetUserId: userWithPermissions.Id,
            targetTenantId: userWithPermissions.TenantId,
            targetEmail: userWithPermissions.Email.Value,
            targetPermissions: aggregatedPermissions,
            sessionId: Guid.CreateVersion7(),
            reason: cmd.Reason,
            now);

        return Result<ImpersonateUserCommandResponse>.Success(
            new ImpersonateUserCommandResponse(
                impersonationToken.Token,
                impersonationToken.ExpiresAt));
    }
}
