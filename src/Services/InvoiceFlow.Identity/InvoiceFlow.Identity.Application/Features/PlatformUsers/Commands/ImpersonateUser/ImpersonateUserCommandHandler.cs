using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class ImpersonateUserCommandHandler(
    IPlatformUsersRepository platformUsersRepository,
    IUsersRepository usersRepository,
    ITokenService tokenService,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<ImpersonateUserCommand, Result<ImpersonateUserCommandResponse>>
{
    public async Task<Result<ImpersonateUserCommandResponse>> Handle(ImpersonateUserCommand cmd, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;

        var existingPlatfromUser = await platformUsersRepository.FindUserByIdAsync(cmd.ActorUserId, cancellationToken);
        if (existingPlatfromUser is null)
            return Result<ImpersonateUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var existingUser = await usersRepository.FindUserByEmailAsync(cmd.TargetUserTenantId, UserEmail.Create(cmd.TargetUserEmail), cancellationToken);
        if (existingUser is null)
            return Result<ImpersonateUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var userWithPermissions = await usersRepository.FindUserByIdWithPermissionsAsync(existingUser.TenantId, existingUser.Id, cancellationToken);

        if (userWithPermissions is null)
            return Result<ImpersonateUserCommandResponse>.Failure(ApplicationErrors.UserUnauthorized);

        var aggregatedPermissions = userWithPermissions.UserRoles.Select(ur => ur.Role.Permissions)
            .Aggregate(SystemPermission.None, (current, rolePermissions) => current | rolePermissions.Value);

        var impersonationToken = tokenService.GenerateImpersonationToken(
            platformUserId: existingPlatfromUser.Id,
            platformEmail: existingPlatfromUser.Email.Value,
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
