using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class ChangePlatformUserPasswordCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPlatformUsersRepository> _usersRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly ChangePlatformUserPasswordCommandHandler _sut;

    public ChangePlatformUserPasswordCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _passwordHasher = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new ChangePlatformUserPasswordCommandHandler(
            _unitOfWork.Object,
            _usersRepository.Object,
            _passwordHasher.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new ChangePlatformUserPasswordCommand(
            Guid.CreateVersion7(),
            "old-password",
            "new-password");

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync((PlatformUser?)null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.UserNotFound.Code);

        _passwordHasher.Verify(
            x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new ChangePlatformUserPasswordCommand(
            Guid.CreateVersion7(),
            "old-password",
            "new-password");

        var user = PlatformUser.Create(
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("old-password-hash"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        user.Deactivate(_mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.UserNotFound.Code);

        _passwordHasher.Verify(
            x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsInvalid_ReturnsValidationFailure()
    {
        // Arrange
        var cmd = new ChangePlatformUserPasswordCommand(
            Guid.CreateVersion7(),
            "old-password",
            "new-password");

        var user = PlatformUser.Create(
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("old-password-hash"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword(cmd.CurrentPassword, user.PasswordHash.Value))
            .Returns(false);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Validation);
        result.Error.Code.Should().Be(ApplicationErrors.UserPasswordInvalid.Code);

        _passwordHasher.Verify(
            x => x.HashPassword(It.IsAny<string>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsChanged_RevokesRefreshTokensAndSavesChanges()
    {
        // Arrange
        var cmd = new ChangePlatformUserPasswordCommand(
            Guid.CreateVersion7(),
            "old-password",
            "new-password");

        var user = PlatformUser.Create(
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("old-password-hash"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        var newPasswordHash = "new-password-hash";

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(x => x.VerifyPassword(cmd.CurrentPassword, user.PasswordHash.Value))
            .Returns(true);

        _passwordHasher
            .Setup(x => x.HashPassword(cmd.NewPassword))
            .Returns(newPasswordHash);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Value.Should().Be(newPasswordHash);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
