using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class LoginPlatformUserCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPlatformRefreshTokensRepository> _refreshTokensRepository;
    private readonly Mock<IPlatformUsersRepository> _usersRepository;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private readonly JwtSettings _jwtSettings = new()
    {
        RefreshTokenDays = 30
    };

    private readonly LoginPlatformUserCommandHandler _sut;

    public LoginPlatformUserCommandTests()
    {
        _unitOfWork = new();
        _refreshTokensRepository = new();
        _usersRepository = new();
        _tokenService = new();
        _passwordHasher = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);

        _sut = new LoginPlatformUserCommandHandler(
            Options.Create(_jwtSettings),
            _unitOfWork.Object,
            _refreshTokensRepository.Object,
            _usersRepository.Object,
            _tokenService.Object,
            _passwordHasher.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new LoginPlatformUserCommand(
            "test@email.com",
            "password",
            null,
            null);

        _usersRepository
            .Setup(x => x.FindUserByEmailAsync(It.IsAny<UserEmail>(), CancellationToken.None))
            .ReturnsAsync((PlatformUser?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _passwordHasher.Verify(
            x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = CreateUser("test@email.com", "hashed-password");

        var command = new LoginPlatformUserCommand(
            "test@email.com",
            "wrong-password",
            null,
            null);

        _usersRepository
            .Setup(x => x.FindUserByEmailAsync(It.IsAny<UserEmail>(), CancellationToken.None))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword("wrong-password", "hashed-password"))
            .Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.UserUnauthorized.Code);

        _refreshTokensRepository.Verify(
            x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveChangesFails()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var user = CreateUser(
            "test@email.com",
            "hashed-password");

        var command = new LoginPlatformUserCommand(
            "test@email.com",
            "password",
            "Chrome",
            "127.0.0.1");

        _usersRepository
            .Setup(x => x.FindUserByEmailAsync(It.IsAny<UserEmail>(), CancellationToken.None))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword("password", "hashed-password"))
            .Returns(true);

        _tokenService
            .Setup(x => x.GeneratePlatformUserAccessToken(
                user.Id,
                "test@email.com",
                It.IsAny<DateTime>()))
            .Returns(("access-token", "Bearer", _mockDateTimeProvider.Object.Now.AddMinutes(15)));

        _tokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            "save.failed",
            "Save failed.");

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Failure(saveError));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);

        _refreshTokensRepository.Verify(
            x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var expiresAt = _mockDateTimeProvider.Object.Now.AddMinutes(15);

        var user = CreateUser(
            "test@email.com",
            "hashed-password");

        var command = new LoginPlatformUserCommand(
            "test@email.com",
            "password",
            "Chrome",
            "127.0.0.1");

        _usersRepository
            .Setup(x => x.FindUserByEmailAsync(It.IsAny<UserEmail>(), CancellationToken.None))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword("password", "hashed-password"))
            .Returns(true);

        _tokenService
            .Setup(x => x.GeneratePlatformUserAccessToken(
                user.Id,
                "test@email.com",
                It.IsAny<DateTime>()))
            .Returns(("access-token", "Bearer", expiresAt));

        _tokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        PlatformRefreshToken? capturedRefreshToken = null;

        _refreshTokensRepository
            .Setup(x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()))
            .Callback<PlatformRefreshToken>(x => capturedRefreshToken = x);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

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
            x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()),
            Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    private PlatformUser CreateUser(
        string email,
        string passwordHash)
    {
        return PlatformUser.Create(
            Guid.CreateVersion7(),
            UserEmail.Create(email),
            PasswordHash.Create(passwordHash),
            FirstName.Create("Test"),
            LastName.Create("User"),
            _mockDateTimeProvider.Object.Now);
    }
}
