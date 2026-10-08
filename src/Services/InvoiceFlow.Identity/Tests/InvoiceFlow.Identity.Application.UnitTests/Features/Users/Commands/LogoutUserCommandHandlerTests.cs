using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class LogoutUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IRefreshTokensRepository> _refreshTokensRepository;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private LogoutUserCommandHandler _sut;

    public LogoutUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _refreshTokensRepository = new();
        _tokenService = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new LogoutUserCommandHandler(
            _unitOfWork.Object,
            _refreshTokensRepository.Object,
            _tokenService.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRefreshTokenDoesNotExist()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var command = new LogoutUserCommand(
            tenantId,
            "refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(
                tenantId,
                It.IsAny<RefreshTokenHash>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRefreshTokenIsAlreadyRevoked()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var existingToken = CreateRefreshToken(tenantId);
        existingToken.Revoke(_mockDateTimeProvider.Object.Now);

        var command = new LogoutUserCommand(
            tenantId,
            "refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(
                tenantId,
                It.IsAny<RefreshTokenHash>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRevokeRefreshToken_WhenRefreshTokenExistsAndIsNotRevoked()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var existingToken = CreateRefreshToken(tenantId);

        var command = new LogoutUserCommand(
            tenantId,
            "refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("refresh-token"))
            .Returns("refresh-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(
                tenantId,
                It.IsAny<RefreshTokenHash>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync(existingToken);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        existingToken.IsRevoked.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCalculateHashFromProvidedRefreshToken()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var command = new LogoutUserCommand(
            tenantId,
            "raw-refresh-token");

        _tokenService
            .Setup(x => x.CalculateTokenHash("raw-refresh-token"))
            .Returns("calculated-token-hash");

        _refreshTokensRepository
            .Setup(x => x.GetTrackedRefreshTokenAsync(
                tenantId,
                It.IsAny<RefreshTokenHash>(),
                TestContext.Current.CancellationToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _tokenService.Verify(
            x => x.CalculateTokenHash("raw-refresh-token"),
            Times.Once);
    }

    private RefreshToken CreateRefreshToken(Guid tenantId)
    {
        return new RefreshToken(
            tenantId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            RefreshTokenHash.Create("refresh-token-hash"),
            _mockDateTimeProvider.Object.Now.AddDays(30),
            _mockDateTimeProvider.Object.Now,
            DeviceInfo.CreateOptional("Chrome"),
            IpAddress.CreateOptional("127.0.0.1"));
    }
}