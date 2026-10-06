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
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new RefreshTokenCommandHandler(
            Options.Create(_jwtSettings),
            _unitOfWork.Object,
            _refreshTokensRepository.Object,
            _usersRepository.Object,
            _tokenService.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenDoesNotExist()
    {
        var tenantId = Guid.CreateVersion7();

        var command = new RefreshTokenCommand(
            tenantId,
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.FindRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenNotFound.Code);

        _usersRepository.Verify(
            x => x.FindUserByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsExpired()
    {
        var tenantId = Guid.CreateVersion7();

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
            .Setup(x => x.FindRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenInvalid.Code);

        _usersRepository.Verify(
            x => x.FindUserByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsRevoked()
    {
        var tenantId = Guid.CreateVersion7();

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
            .Setup(x => x.FindRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenInvalid.Code);

        _usersRepository.Verify(
            x => x.FindUserByIdWithPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var tenantId = Guid.CreateVersion7();

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
            .Setup(x => x.FindRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        _usersRepository
            .Setup(x => x.FindUserByIdWithPermissionsAsync(tenantId, existingToken.UserId, TestContext.Current.CancellationToken))
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
    public async Task Handle_ShouldReturnFailure_WhenSaveChangesFails()
    {
        var tenantId = Guid.CreateVersion7();

        var existingToken = CreateRefreshToken(
            tenantId,
            expiresAt: _mockDateTimeProvider.Object.Now.AddDays(30));

        var user = CreateUserWithPermissions(
            tenantId,
            existingToken.UserId,
            "test@email.com",
            SystemPermission.CustomerView);

        var command = new RefreshTokenCommand(
            tenantId,
            "old-refresh-token",
            "Chrome",
            "127.0.0.1");

        SetupSuccessfulTokenFlow(
            tenantId,
            existingToken,
            user,
            command);

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Failure(saveError));

        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);

        _refreshTokensRepository.Verify(
            x => x.AddRefreshToken(It.IsAny<RefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRefreshTokenIsValid()
    {
        var tenantId = Guid.CreateVersion7();
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
            .Setup(x => x.FindRefreshTokenAsync(tenantId, It.IsAny<RefreshTokenHash>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        _usersRepository
            .Setup(x => x.FindUserByIdWithPermissionsAsync(tenantId, existingToken.UserId, TestContext.Current.CancellationToken))
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
        // Adjust this helper to your actual User / Role constructors.
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
}