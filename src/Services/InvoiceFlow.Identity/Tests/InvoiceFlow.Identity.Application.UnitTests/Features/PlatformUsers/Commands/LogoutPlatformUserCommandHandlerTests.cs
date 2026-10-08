using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class LogoutPlatformUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPlatformRefreshTokensRepository> _refreshTokensRepository;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly LogoutPlatformUserCommandHandler _sut;

    public LogoutPlatformUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _refreshTokensRepository = new();
        _tokenService = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);

        _sut = new LogoutPlatformUserCommandHandler(
            _unitOfWork.Object,
            _refreshTokensRepository.Object,
            _tokenService.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRefreshTokenDoesNotExist()
    {
        // Arrange
        var command = new LogoutPlatformUserCommand("refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync((PlatformRefreshToken?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRefreshTokenIsAlreadyRevoked()
    {
        // Arrange
        var existingToken = CreateRefreshToken();
        existingToken.Revoke(_mockDateTimeProvider.Object.Now);

        var command = new LogoutPlatformUserCommand("refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync(existingToken);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRevokeRefreshToken_WhenRefreshTokenExistsAndIsNotRevoked()
    {
        // Arrange
        var existingToken = CreateRefreshToken();

        var command = new LogoutPlatformUserCommand("refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync(existingToken);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        existingToken.IsRevoked.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCalculateHashFromProvidedRefreshToken()
    {
        // Arrange
        var command = new LogoutPlatformUserCommand("raw-refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("raw-refresh-token"))
            .Returns("calculated-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedPlatformRefreshTokenAsync(It.IsAny<RefreshTokenHash>(), CancellationToken.None))
            .ReturnsAsync((PlatformRefreshToken?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _tokenService.Verify(
            x => x.CalculateTokenHash("raw-refresh-token"),
            Times.Once);
    }

    private PlatformRefreshToken CreateRefreshToken()
    {
        return PlatformRefreshToken.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            RefreshTokenHash.Create("refresh-token-hash"),
            _mockDateTimeProvider.Object.Now.AddDays(30),
            _mockDateTimeProvider.Object.Now,
            DeviceInfo.CreateOptional("Chrome"),
            IpAddress.CreateOptional("127.0.0.1"));
    }
}
