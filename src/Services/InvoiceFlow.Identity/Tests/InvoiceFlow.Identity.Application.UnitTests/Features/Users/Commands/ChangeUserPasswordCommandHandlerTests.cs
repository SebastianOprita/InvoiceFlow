using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class ChangeUserPasswordCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<IRefreshTokensRepository> _refreshTokensRepository;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private ChangeUserPasswordCommandHandler _sut;

    public ChangeUserPasswordCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _refreshTokensRepository = new();
        _passwordHasher = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new ChangeUserPasswordCommandHandler(
            _unitOfWork.Object,
            _usersRepository.Object,
            _refreshTokensRepository.Object,
            _passwordHasher.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new ChangeUserPasswordCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "old-password",
            "new-password");

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.UserNotFound.Code);

        _passwordHasher.Verify(
            x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.RevokeAccessForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTime>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new ChangeUserPasswordCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "old-password",
            "new-password");

        var user = User.Create(
            cmd.TenantId,
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("old-password-hash"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        user.Deactivate(_mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.UserNotFound.Code);

        _passwordHasher.Verify(
            x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.RevokeAccessForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsInvalid_ReturnsValidationFailure()
    {
        // Arrange
        var cmd = new ChangeUserPasswordCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "old-password",
            "new-password");

        var user = User.Create(
            cmd.TenantId,
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("old-password-hash"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword(cmd.CurrentPassword, user.PasswordHash.Value))
            .Returns(false);

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Validation);
        result.Error.Code.Should().Be(ApplicationErrors.UserPasswordInvalid.Code);

        _passwordHasher.Verify(
            x => x.HashPassword(It.IsAny<string>()),
            Times.Never);

        _refreshTokensRepository.Verify(
            x => x.RevokeAccessForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateTime>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(TestContext.Current.CancellationToken), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsChanged_RevokesRefreshTokensAndSavesChanges()
    {
        // Arrange
        var cmd = new ChangeUserPasswordCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "old-password",
            "new-password");

        var user = User.Create(
            cmd.TenantId,
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("old-password-hash"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        var newPasswordHash = "new-password-hash";

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword(cmd.CurrentPassword, user.PasswordHash.Value))
            .Returns(true);

        _passwordHasher
            .Setup(x => x.HashPassword(cmd.NewPassword))
            .Returns(newPasswordHash);

        _refreshTokensRepository
            .Setup(x => x.RevokeAccessForUserAsync(
                cmd.TenantId,
                cmd.UserId,
                It.IsAny<DateTime>(),
                TestContext.Current.CancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Value.Should().Be(newPasswordHash);

        _refreshTokensRepository.Verify(
            x => x.RevokeAccessForUserAsync(
                cmd.TenantId,
                cmd.UserId,
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
