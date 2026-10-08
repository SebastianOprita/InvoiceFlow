using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IRefreshTokensRepository> _refreshTokensRepository;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<ITenantsRepository> _tenantsRepository;

    private readonly JwtSettings _jwtSettings = new()
    {
        RefreshTokenDays = 30
    };

    private RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _unitOfWork = new();
        _refreshTokensRepository = new();
        _usersRepository = new();
        _tokenService = new();
        _mockDateTimeProvider = new();
        _tenantsRepository = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new RefreshTokenCommandHandler(
            Options.Create(_jwtSettings),
            _unitOfWork.Object,
            _refreshTokensRepository.Object,
            _tenantsRepository.Object,
            _usersRepository.Object,
            _tokenService.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenDoesNotExist()
    {
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var command = new RefreshTokenCommand(
            tenantId,
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenInvalid.Code);

        _usersRepository.Verify(
            x => x.GetUserByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsExpired()
    {
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var existingToken = CreateRefreshToken(
            tenantId,
            expiresAt: _mockDateTimeProvider.Object.Now.AddDays(-1));

        var command = new RefreshTokenCommand(
            tenantId,
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenInvalid.Code);

        _usersRepository.Verify(
            x => x.GetUserByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsRevoked()
    {
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var existingToken = CreateRefreshToken(
            tenantId,
            expiresAt: _mockDateTimeProvider.Object.Now.AddDays(30));

        existingToken.Revoke(_mockDateTimeProvider.Object.Now);

        var command = new RefreshTokenCommand(
            tenantId,
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenInvalid.Code);

        _usersRepository.Verify(
            x => x.GetUserByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var existingToken = CreateRefreshToken(
            tenantId,
            expiresAt: _mockDateTimeProvider.Object.Now.AddDays(30));

        var command = new RefreshTokenCommand(
            tenantId,
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        _usersRepository
            .Setup(x => x.GetUserByIdWithPermissionsAsync(tenantId, existingToken.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync((User?)null);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.UserNotFound.Code);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRefreshTokenIsValid()
    {
        var tenantId = Guid.CreateVersion7();
        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(
                tenantId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));

        var accessTokenExpiresAt = _mockDateTimeProvider.Object.Now.AddMinutes(15);

        var existingToken = CreateRefreshToken(
            tenantId,
            expiresAt: _mockDateTimeProvider.Object.Now.AddDays(30));

        var user = CreateUserWithPermissions(
            tenantId,
            existingToken.UserId,
            "test@email.com",
            SystemPermission.CustomerView | SystemPermission.CustomerCreate);

        var command = new RefreshTokenCommand(
            tenantId,
            "old-refresh-token",
            "Chrome",
            "127.0.0.1");

        SetupSuccessfulTokenFlow(
            tenantId,
            existingToken,
            user,
            command,
            accessTokenExpiresAt);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        RefreshToken? capturedNewRefreshToken = null;

        _refreshTokensRepository
            .Setup(x => x.AddRefreshToken(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(x => capturedNewRefreshToken = x);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.AccessTokenExpiresAt.Should().Be(accessTokenExpiresAt);
        result.Value.RefreshToken.Should().Be("new-refresh-token");
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Email.Should().Be("test@email.com");

        capturedNewRefreshToken.Should().NotBeNull();
        existingToken.IsRevoked.Should().BeTrue();
        existingToken.ReplacedByTokenId.Should().Be(capturedNewRefreshToken!.Id);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupSuccessfulTokenFlow(
        Guid tenantId,
        RefreshToken existingToken,
        User user,
        RefreshTokenCommand command,
        DateTime? accessTokenExpiresAt = null)
    {
        accessTokenExpiresAt ??= _mockDateTimeProvider.Object.Now.AddMinutes(15);

        _tokenService
            .Setup(x => x.CalculateTokenHash(command.RefreshToken))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        _usersRepository
            .Setup(x => x.GetUserByIdWithPermissionsAsync(tenantId, existingToken.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _tokenService
            .Setup(x => x.GenerateUserAccessToken(
                user.Id,
                tenantId,
                user.Email.Value,
                It.IsAny<SystemPermission>(),
                It.IsAny<DateTime>()))
            .Returns(("new-access-token", "Bearer", accessTokenExpiresAt.Value));

        _tokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("new-refresh-token"))
            .Returns("new-refresh-token-hash");
    }

    private RefreshToken CreateRefreshToken(
        Guid tenantId,
        DateTime expiresAt)
    {
        return new RefreshToken(
            tenantId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            RefreshTokenHash.Create("old-refresh-token-hash"),
            expiresAt,
            _mockDateTimeProvider.Object.Now.AddDays(-2),
            DeviceInfo.CreateOptional("Chrome"),
            IpAddress.CreateOptional("127.0.0.1"));
    }

    private User CreateUserWithPermissions(
        Guid tenantId,
        Guid userId,
        string email,
        SystemPermission permissions)
    {
        var user = User.Create(
            tenantId,
            userId,
            UserEmail.Create(email),
            PasswordHash.Create("hashed-password"),
            FirstName.Create("Test"),
            LastName.Create("User"),
            _mockDateTimeProvider.Object.Now);

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



    private Tenant CreateTenant(Guid tenantId)
    {
        return Tenant.Create(
            tenantId,
            TenantName.Create("Test Tenant"),
            TenantSlug.Create("test-tenant"),
            _mockDateTimeProvider.Object.Now);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTenantDoesNotExist()
    {
        var tenantId = Guid.CreateVersion7();
        var command = new RefreshTokenCommand(tenantId, "old-refresh-token", null, null);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenInvalid.Code);
        VerifyNoTokenRotation();
        _refreshTokensRepository.Verify(
            x => x.GetTrackedRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<RefreshTokenHash>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTenantIsInactive()
    {
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId);
        tenant.Deactivate(_mockDateTimeProvider.Object.Now);
        var command = new RefreshTokenCommand(tenantId, "old-refresh-token", null, null);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenInvalid.Code);
        VerifyNoTokenRotation();
        _refreshTokensRepository.Verify(
            x => x.GetTrackedRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<RefreshTokenHash>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsInactive()
    {
        var tenantId = Guid.CreateVersion7();
        var now = _mockDateTimeProvider.Object.Now;
        var existingToken = CreateRefreshToken(tenantId, now.AddDays(30));
        var user = CreateUserWithPermissions(
            tenantId, existingToken.UserId, "test@email.com", SystemPermission.CustomerView);
        user.Deactivate(now);
        var command = new RefreshTokenCommand(tenantId, "old-refresh-token", null, null);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId));
        _tokenService.Setup(x => x.CalculateTokenHash(command.RefreshToken))
            .Returns("old-refresh-token-hash");
        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(
                tenantId, It.IsAny<RefreshTokenHash>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);
        _usersRepository
            .Setup(x => x.GetUserByIdWithPermissionsAsync(
                tenantId, existingToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.UserNotFound.Code);
        existingToken.IsRevoked.Should().BeFalse();
        VerifyNoTokenRotation();
    }

    private void VerifyNoTokenRotation()
    {
        _tokenService.Verify(x => x.GenerateRefreshToken(), Times.Never);
        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
