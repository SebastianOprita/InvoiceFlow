using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class RefreshPlatformTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPlatformRefreshTokensRepository> _refreshTokensRepository;
    private readonly Mock<IPlatformUsersRepository> _usersRepository;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private readonly JwtSettings _jwtSettings = new()
    {
        RefreshTokenDays = 30
    };

    private readonly RefreshPlatformTokenCommandHandler _sut;

    public RefreshPlatformTokenCommandHandlerTests()
    {
        _unitOfWork = new();
        _refreshTokensRepository = new();
        _usersRepository = new();
        _tokenService = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);

        _sut = new RefreshPlatformTokenCommandHandler(
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
        var command = new RefreshPlatformTokenCommand(
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.FindPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync((PlatformRefreshToken?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenNotFound.Code);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsExpired()
    {
        var existingToken = CreateRefreshToken(_mockDateTimeProvider.Object.Now.AddDays(-1));

        var command = new RefreshPlatformTokenCommand(
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.FindPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync(existingToken);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenNotFound.Code);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsRevoked()
    {
        var existingToken = CreateRefreshToken(_mockDateTimeProvider.Object.Now.AddDays(30));

        existingToken.Revoke(_mockDateTimeProvider.Object.Now);

        var command = new RefreshPlatformTokenCommand(
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.FindPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync(existingToken);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenNotFound.Code);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var existingToken = CreateRefreshToken(_mockDateTimeProvider.Object.Now.AddDays(30));

        var command = new RefreshPlatformTokenCommand(
            "old-refresh-token",
            null,
            null);

        _tokenService
            .Setup(x => x.CalculateTokenHash("old-refresh-token"))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.FindPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync(existingToken);

        _usersRepository
            .Setup(x => x.FindUserByIdAsync(existingToken.UserId, CancellationToken.None))
            .ReturnsAsync((PlatformUser?)null);

        _refreshTokensRepository
            .Setup(x => x.GetPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Unauthorized);
        result.Error.Code.Should().Be(ApplicationErrors.RefreshTokenInvalid.Code);

        _refreshTokensRepository.Verify(
            x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveChangesFails()
    {
        var existingToken = CreateRefreshToken(_mockDateTimeProvider.Object.Now.AddDays(30));

        var user = CreateUser(
            existingToken.UserId,
            "test@email.com");

        var command = new RefreshPlatformTokenCommand(
            "old-refresh-token",
            "Chrome",
            "127.0.0.1");

        _refreshTokensRepository
            .Setup(x => x.GetPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        SetupSuccessfulTokenFlow(
            existingToken,
            user,
            command);

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .ReturnsAsync(Result.Failure(saveError));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);

        _refreshTokensRepository.Verify(
            x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRefreshTokenIsValid()
    {
        var accessTokenExpiresAt = _mockDateTimeProvider.Object.Now.AddMinutes(15);

        var existingToken = CreateRefreshToken(_mockDateTimeProvider.Object.Now.AddDays(30));

        var user = CreateUser(
            existingToken.UserId,
            "test@email.com");

        var command = new RefreshPlatformTokenCommand(
            "old-refresh-token",
            "Chrome",
            "127.0.0.1");

        SetupSuccessfulTokenFlow(
            existingToken,
            user,
            command,
            accessTokenExpiresAt);

        _refreshTokensRepository
            .Setup(x => x.GetPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingToken);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        PlatformRefreshToken? capturedNewRefreshToken = null;

        _refreshTokensRepository
            .Setup(x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()))
            .Callback<PlatformRefreshToken>(x => capturedNewRefreshToken = x);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.AccessTokenExpiresAt.Should().Be(accessTokenExpiresAt);
        result.Value.RefreshToken.Should().Be("new-refresh-token");
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Email.Should().Be("test@email.com");

        capturedNewRefreshToken.Should().NotBeNull();

        _refreshTokensRepository.Verify(
            x => x.AddPlatformRefreshToken(It.IsAny<PlatformRefreshToken>()),
            Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    private void SetupSuccessfulTokenFlow(
        PlatformRefreshToken existingToken,
        PlatformUser user,
        RefreshPlatformTokenCommand command,
        DateTime? accessTokenExpiresAt = null)
    {
        accessTokenExpiresAt ??= _mockDateTimeProvider.Object.Now.AddMinutes(15);

        _tokenService
            .Setup(x => x.CalculateTokenHash(command.RefreshToken))
            .Returns("old-refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.FindPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync(existingToken);

        _usersRepository
            .Setup(x => x.FindUserByIdAsync(existingToken.UserId, CancellationToken.None))
            .ReturnsAsync(user);

        _tokenService
            .Setup(x => x.GeneratePlatformUserAccessToken(
                user.Id,
                user.Email.Value,
                It.IsAny<DateTime>()))
            .Returns(("new-access-token", "Bearer", accessTokenExpiresAt.Value));

        _tokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("new-refresh-token"))
            .Returns("new-refresh-token-hash");
    }

    private PlatformRefreshToken CreateRefreshToken(DateTime expiresAt)
    {
        return PlatformRefreshToken.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            RefreshTokenHash.Create("old-refresh-token-hash"),
            expiresAt,
            _mockDateTimeProvider.Object.Now.AddDays(-2),
            DeviceInfo.CreateOptional("Chrome"),
            IpAddress.CreateOptional("127.0.0.1"));
    }

    private PlatformUser CreateUser(
        Guid userId,
        string email)
    {
        var user = PlatformUser.Create(
            userId,
            UserEmail.Create(email),
            PasswordHash.Create("hashed-password"),
            FirstName.Create("Test"),
            LastName.Create("User"),
            _mockDateTimeProvider.Object.Now);

        return user;
    }
}
