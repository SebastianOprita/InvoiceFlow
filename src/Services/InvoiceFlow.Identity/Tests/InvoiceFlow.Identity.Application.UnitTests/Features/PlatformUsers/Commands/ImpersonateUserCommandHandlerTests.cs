using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class ImpersonateUserCommandHandlerTests
{
    private readonly Mock<IPlatformUsersRepository> _platformUsersRepository;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly ImpersonateUserCommandHandler _sut;

    public ImpersonateUserCommandHandlerTests()
    {
        _platformUsersRepository = new();
        _usersRepository = new();
        _tokenService = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new ImpersonateUserCommandHandler(
            _platformUsersRepository.Object,
            _usersRepository.Object,
            _tokenService.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_Platform_User_Not_Found()
    {
        var command = ValidCommand();

        _platformUsersRepository
            .Setup(x => x.FindUserByIdAsync(command.ActorUserId, CancellationToken.None))
            .ReturnsAsync((PlatformUser?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        result.Error.Should().NotBeNull();
        Assert.Equal(ApplicationErrorType.Unauthorized, result.Error.Type);
        Assert.Equal(ApplicationErrors.UserUnauthorized.Code, result.Error.Code);

        _usersRepository.Verify(
            x => x.FindUserByEmailAsync(It.IsAny<Guid>(), It.IsAny<UserEmail>(), CancellationToken.None),
            Times.Never);

        _tokenService.Verify(
            x => x.GenerateImpersonationToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<SystemPermission>(),
                It.IsAny<Guid>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_Target_User_Not_Found_By_Email()
    {
        var command = ValidCommand();
        var platformUser = CreatePlatformUser(command.ActorUserId);

        _platformUsersRepository
            .Setup(x => x.FindUserByIdAsync(command.ActorUserId, CancellationToken.None))
            .ReturnsAsync(platformUser);

        _usersRepository
            .Setup(x => x.FindUserByEmailAsync(
                command.TargetUserTenantId,
                It.Is<UserEmail>(email => email.Value == command.TargetUserEmail),
                CancellationToken.None))
            .ReturnsAsync((User?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        result.Error.Should().NotBeNull();
        Assert.Equal(ApplicationErrorType.Unauthorized, result.Error.Type);
        Assert.Equal(ApplicationErrors.UserUnauthorized.Code, result.Error.Code);

        _usersRepository.Verify(
            x => x.FindUserByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), CancellationToken.None),
            Times.Never);

        _tokenService.Verify(
            x => x.GenerateImpersonationToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<SystemPermission>(),
                It.IsAny<Guid>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_Target_User_With_Permissions_Not_Found()
    {
        var command = ValidCommand();
        var platformUser = CreatePlatformUser(command.ActorUserId);
        var targetUser = CreateUser(command.TargetUserTenantId, Guid.CreateVersion7());

        _platformUsersRepository
            .Setup(x => x.FindUserByIdAsync(command.ActorUserId, CancellationToken.None))
            .ReturnsAsync(platformUser);

        _usersRepository
            .Setup(x => x.FindUserByEmailAsync(
                command.TargetUserTenantId,
                It.Is<UserEmail>(email => email.Value == command.TargetUserEmail),
                CancellationToken.None))
            .ReturnsAsync(targetUser);

        _usersRepository
            .Setup(x => x.FindUserByIdWithPermissionsAsync(targetUser.TenantId, targetUser.Id, CancellationToken.None))
            .ReturnsAsync((User?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        result.Error.Should().NotBeNull();
        Assert.Equal(ApplicationErrorType.Unauthorized, result.Error.Type);
        Assert.Equal(ApplicationErrors.UserUnauthorized.Code, result.Error.Code);

        _tokenService.Verify(
            x => x.GenerateImpersonationToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<SystemPermission>(),
                It.IsAny<Guid>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Generate_Impersonation_Token_When_Command_Is_Valid()
    {
        var tenantId = Guid.CreateVersion7();
        var command = ValidCommand();

        var platformUser = CreatePlatformUser(command.ActorUserId);
        var targetUser = CreateUser(command.TargetUserTenantId, Guid.CreateVersion7());
        var targetUserWithPermissions = CreateUserWithPermissions(
            targetUser.TenantId,
            targetUser.Id,
            SystemPermission.ManageUsers | SystemPermission.ManageRoles);

        var expectedToken = "impersonation-token";
        var expectedExpiresAt = _mockDateTimeProvider.Object.Now.AddMinutes(15);

        _platformUsersRepository
            .Setup(x => x.FindUserByIdAsync(command.ActorUserId, CancellationToken.None))
            .ReturnsAsync(platformUser);

        _usersRepository
            .Setup(x => x.FindUserByEmailAsync(
                command.TargetUserTenantId,
                It.Is<UserEmail>(email => email.Value == command.TargetUserEmail),
                CancellationToken.None))
            .ReturnsAsync(targetUser);

        _usersRepository
            .Setup(x => x.FindUserByIdWithPermissionsAsync(targetUser.TenantId, targetUser.Id, CancellationToken.None))
            .ReturnsAsync(targetUserWithPermissions);

        _tokenService
            .Setup(x => x.GenerateImpersonationToken(
                platformUser.Id,
                platformUser.Email.Value,
                targetUserWithPermissions.Id,
                targetUserWithPermissions.TenantId,
                targetUserWithPermissions.Email.Value,
                SystemPermission.ManageUsers | SystemPermission.ManageRoles,
                It.IsAny<Guid>(),
                command.Reason,
                It.IsAny<DateTime>()))
            .Returns((expectedToken, "Bearer", expectedExpiresAt));

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        result.Value.Should().NotBeNull();
        Assert.Equal(expectedToken, result.Value.ImpersonationToken);
        Assert.Equal(expectedExpiresAt, result.Value.ImpersonationTokenExpiresAt);
    }

    [Fact]
    public async Task Handle_Should_Pass_Aggregated_Permissions_To_Token_Service()
    {
        var command = ValidCommand();

        var platformUser = CreatePlatformUser(command.ActorUserId);
        var targetUser = CreateUser(command.TargetUserTenantId, Guid.CreateVersion7());
        var targetUserWithPermissions = CreateUserWithPermissions(
            targetUser.TenantId,
            targetUser.Id,
            SystemPermission.ManageUsers | SystemPermission.ManageRoles);

        _platformUsersRepository
            .Setup(x => x.FindUserByIdAsync(command.ActorUserId, CancellationToken.None))
            .ReturnsAsync(platformUser);

        _usersRepository
            .Setup(x => x.FindUserByEmailAsync(
                command.TargetUserTenantId,
                It.Is<UserEmail>(email => email.Value == command.TargetUserEmail),
                CancellationToken.None))
            .ReturnsAsync(targetUser);

        _usersRepository
            .Setup(x => x.FindUserByIdWithPermissionsAsync(targetUser.TenantId, targetUser.Id, CancellationToken.None))
            .ReturnsAsync(targetUserWithPermissions);

        _tokenService
            .Setup(x => x.GenerateImpersonationToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<SystemPermission>(),
                It.IsAny<Guid>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime>()))
            .Returns(("token", "Bearer", _mockDateTimeProvider.Object.Now.AddMinutes(15)));

        await _sut.Handle(command, CancellationToken.None);

        _tokenService.Verify(x => x.GenerateImpersonationToken(
            platformUser.Id,
            platformUser.Email.Value,
            targetUserWithPermissions.Id,
            targetUserWithPermissions.TenantId,
            targetUserWithPermissions.Email.Value,
            SystemPermission.ManageUsers | SystemPermission.ManageRoles,
            It.IsAny<Guid>(),
            command.Reason,
            It.IsAny<DateTime>()),
            Times.Once);
    }

    private static ImpersonateUserCommand ValidCommand() =>
        new(
            ActorUserId: Guid.CreateVersion7(),
            TargetUserTenantId: Guid.CreateVersion7(),
            TargetUserEmail: "target@example.com",
            Reason: "Support request",
            DeviceInfo: "Chrome",
            IpAddress: "127.0.0.1");

    private User CreateUser(
        Guid tenantId,
        Guid id)
    {
        // Adjust to your actual User factory/constructor.
        return User.Create(
            tenantId,
            id,
            UserEmail.Create("email@example.com"),
            PasswordHash.Create("passwordHash"),
            FirstName.Create("Test"),
            LastName.Create("User"),
            _mockDateTimeProvider.Object.Now);
    }

    private User CreateUserWithPermissions(
        Guid tenantId,
        Guid id,
        SystemPermission permissions)
    {
        var user = CreateUser(tenantId, id);

        var role = Role.Create(
            tenantId,
            Guid.CreateVersion7(),
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(permissions));

        user.AssignRole(role.Id, _mockDateTimeProvider.Object.Now);

        // Manually set the Role navigation property on the UserRole to simulate EF Core's eager loading
        var userRole = ((List<UserRole>)user.UserRoles).First();
        var roleProperty = typeof(UserRole).GetProperty("Role", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        roleProperty?.SetValue(userRole, role);

        return user;
    }

    private PlatformUser CreatePlatformUser(Guid id)
    {
        return PlatformUser.Create(
            id,
            UserEmail.Create("platform@example.com"),
            PasswordHash.Create("passwordHash"),
            FirstName.Create("Test"),
            LastName.Create("User"),
            _mockDateTimeProvider.Object.Now);
    }
}
