using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public sealed class LoginUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IRefreshTokensRepository> _refreshTokensRepository;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<ITenantsRepository> _tenantsRepository;

    private readonly JwtSettings _jwtSettings = new()
    {
        RefreshTokenDays = 30
    };

    private LoginUserCommandHandler _sut;

    public LoginUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _refreshTokensRepository = new();
        _usersRepository = new();
        _tenantsRepository = new();
        _tokenService = new();
        _passwordHasher = new();
        _mockDateTimeProvider = new();

        _mockDateTimeProvider
            .Setup(x => x.Now)
            .Returns(DateTime.UtcNow);

        _sut = new LoginUserCommandHandler(
            Options.Create(_jwtSettings),
            _unitOfWork.Object,
            _refreshTokensRepository.Object,
            _tenantsRepository.Object,
            _usersRepository.Object,
            _tokenService.Object,
            _passwordHasher.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "password",
            null,
            null);

        _usersRepository
            .Setup(x => x.GetUserByEmailAsync(tenantId, It.IsAny<UserEmail>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _passwordHasher.Verify(
            x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var user = CreateUser(tenantId, "test@email.com", "hashed-password");

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "wrong-password",
            null,
            null);

        _usersRepository
            .Setup(x => x.GetUserByEmailAsync(tenantId, It.IsAny<UserEmail>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword("wrong-password", "hashed-password"))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _usersRepository.Verify(
            x => x.GetUserByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserWithPermissionsDoesNotExist()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var user = CreateUser(tenantId, "test@email.com", "hashed-password");

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "password",
            null,
            null);

        _usersRepository
            .Setup(x => x.GetUserByEmailAsync(tenantId, It.IsAny<UserEmail>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword("password", "hashed-password"))
            .Returns(true);

        _usersRepository
            .Setup(x => x.GetUserByIdWithPermissionsAsync(tenantId, user.Id, TestContext.Current.CancellationToken))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _tokenService.Verify(
            x => x.GenerateUserAccessToken(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<SystemPermission>(),
                It.IsAny<DateTime>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var expiresAt = _mockDateTimeProvider.Object.Now.AddMinutes(15);

        var user = CreateUserWithPermissions(
            tenantId,
            "test@email.com",
            "hashed-password",
            SystemPermission.CustomerView | SystemPermission.CustomerCreate);

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "password",
            "Chrome",
            "127.0.0.1");

        _usersRepository
            .Setup(x => x.GetUserByEmailAsync(tenantId, It.IsAny<UserEmail>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _usersRepository
            .Setup(x => x.GetUserByIdWithPermissionsAsync(tenantId, user.Id, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword("password", "hashed-password"))
            .Returns(true);

        _tokenService
            .Setup(x => x.GenerateUserAccessToken(
                user.Id,
                tenantId,
                "test@email.com",
                SystemPermission.CustomerView | SystemPermission.CustomerCreate,
                It.IsAny<DateTime>()))
            .Returns(("access-token", "Bearer", expiresAt));

        _tokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        RefreshToken? capturedRefreshToken = null;

        _refreshTokensRepository
            .Setup(x => x.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(x => capturedRefreshToken = x);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.AccessTokenExpiresAt.Should().Be(expiresAt);
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Email.Should().Be("test@email.com");

        capturedRefreshToken.Should().NotBeNull();

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTenantDoesNotExist()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "password",
            null,
            null);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _sut.Handle(
            command,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _usersRepository.VerifyNoOtherCalls();

        _tokenService.Verify(
            x => x.GenerateRefreshToken(),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTenantIsInactive()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId);

        // Use your Tenant domain method to deactivate it.
        tenant.Deactivate(_mockDateTimeProvider.Object.Now);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "password",
            null,
            null);

        // Act
        var result = await _sut.Handle(
            command,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _usersRepository.VerifyNoOtherCalls();

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsInactive()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var user = CreateUser(
            tenantId,
            "test@email.com",
            "hashed-password");

        // Use your actual User deactivation method.
        user.Deactivate(_mockDateTimeProvider.Object.Now);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        _usersRepository
            .Setup(x => x.GetUserByEmailAsync(
                tenantId,
                It.IsAny<UserEmail>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "password",
            null,
            null);

        // Act
        var result = await _sut.Handle(
            command,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _passwordHasher.Verify(
            x => x.VerifyPassword(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserWithPermissionsIsInactive()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var user = CreateUser(
            tenantId,
            "test@email.com",
            "hashed-password");

        var inactiveUser = CreateUser(
            tenantId,
            "test@email.com",
            "hashed-password");

        inactiveUser.Deactivate(_mockDateTimeProvider.Object.Now);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        _usersRepository
            .Setup(x => x.GetUserByEmailAsync(
                tenantId,
                It.IsAny<UserEmail>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword(
                "password",
                "hashed-password"))
            .Returns(true);

        _usersRepository
            .Setup(x => x.GetUserByIdWithPermissionsAsync(
                tenantId,
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "password",
            null,
            null);

        // Act
        var result = await _sut.Handle(
            command,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _passwordHasher.Verify(
            x => x.VerifyPassword(
                "password",
                "hashed-password"),
            Times.Once);

        _tokenService.Verify(
            x => x.GenerateUserAccessToken(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<SystemPermission>(),
                It.IsAny<DateTime>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserHasNoRoles()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var user = CreateUser(
            tenantId,
            "test@email.com",
            "hashed-password");

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        _usersRepository
            .Setup(x => x.GetUserByEmailAsync(
                tenantId,
                It.IsAny<UserEmail>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _usersRepository
            .Setup(x => x.GetUserByIdWithPermissionsAsync(
                tenantId,
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword(
                "password",
                "hashed-password"))
            .Returns(true);

        _tokenService
            .Setup(x => x.GenerateUserAccessToken(
                user.Id,
                tenantId,
                user.Email.Value,
                SystemPermission.None,
                It.IsAny<DateTime>()))
            .Returns((
                "access-token",
                "Bearer",
                _mockDateTimeProvider.Object.Now.AddMinutes(15)));

        _tokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new LoginUserCommand(
            tenantId,
            "test@email.com",
            "password",
            null,
            null);

        // Act
        var result = await _sut.Handle(
            command,
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _tokenService.Verify(
            x => x.GenerateUserAccessToken(
                user.Id,
                tenantId,
                user.Email.Value,
                SystemPermission.None,
                It.IsAny<DateTime>()),
            Times.Once);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Once);
    }

    private Tenant CreateTenant(Guid tenantId)
    {
        return Tenant.Create(
            tenantId,
            TenantName.Create("Test Tenant"),
            TenantSlug.Create("test-tenant"),
            _mockDateTimeProvider.Object.Now);
    }

    private User CreateUser(
        Guid tenantId,
        string email,
        string passwordHash)
    {
        return User.Create(
            tenantId,
            Guid.CreateVersion7(),
            UserEmail.Create(email),
            PasswordHash.Create(passwordHash),
            FirstName.Create("Test"),
            LastName.Create("User"),
            _mockDateTimeProvider.Object.Now);
    }

    private User CreateUserWithPermissions(
        Guid tenantId,
        string email,
        string passwordHash,
        SystemPermission permissions)
    {
        var user = CreateUser(tenantId, email, passwordHash);

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
}
