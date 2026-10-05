using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class DeactivatePlatformUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPlatformUsersRepository> _usersRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly DeactivatePlatformUserCommandHandler _sut;

    public DeactivatePlatformUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new DeactivatePlatformUserCommandHandler(
            _unitOfWork.Object,
            _usersRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new DeactivatePlatformUserCommand(Guid.CreateVersion7());

        _usersRepository
            .Setup(x => x.GetUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync((PlatformUser?)null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.UserNotFound.Code);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserIsAlreadyInactive_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        var cmd = new DeactivatePlatformUserCommand(Guid.CreateVersion7());

        var user = PlatformUser.Create(
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        user.Deactivate(_mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserIsActive_DeactivatesUserAndSavesChanges()
    {
        // Arrange
        var ct = CancellationToken.None;

        var cmd = new DeactivatePlatformUserCommand(Guid.CreateVersion7());

        var user = PlatformUser.Create(
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(ct))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();

        _unitOfWork.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var ct = CancellationToken.None;

        var cmd = new DeactivatePlatformUserCommand(Guid.CreateVersion7());

        var user = PlatformUser.Create(
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

        _usersRepository
            .Setup(x => x.GetUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(ct))
            .ReturnsAsync(Result.Failure(saveError));

        // Act
        var result = await _sut.Handle(cmd, ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);

        user.IsActive.Should().BeFalse();

        _unitOfWork.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }
}
