using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class ActivatePlatformUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPlatformUsersRepository> _usersRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly ActivatePlatformUserCommandHandler _sut;

    public ActivatePlatformUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new ActivatePlatformUserCommandHandler(
            _unitOfWork.Object, 
            _usersRepository.Object, 
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new ActivatePlatformUserCommand(Guid.CreateVersion7());

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

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserIsAlreadyActive_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        var cmd = new ActivatePlatformUserCommand(Guid.CreateVersion7());

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

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserIsInactive_ActivatesUserAndSavesChanges()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cmd = new ActivatePlatformUserCommand(Guid.CreateVersion7());

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

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(ct))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var ct = CancellationToken.None;
        var cmd = new ActivatePlatformUserCommand(Guid.CreateVersion7());

        var user = PlatformUser.Create(
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        user.Deactivate(_mockDateTimeProvider.Object.Now);

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
        user.IsActive.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }
}
