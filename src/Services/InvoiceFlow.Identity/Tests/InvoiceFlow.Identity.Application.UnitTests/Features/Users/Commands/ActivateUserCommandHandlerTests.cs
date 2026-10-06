using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class ActivateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private ActivateUserCommandHandler _sut;

    public ActivateUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new ActivateUserCommandHandler(
            _unitOfWork.Object,
            _usersRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new ActivateUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        _usersRepository
            .Setup(x => x.GetUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.UserNotFound.Code);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsAlreadyActive_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        var cmd = new ActivateUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var user = User.Create(
            cmd.TenantId,
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ActivatesUserAndSavesChanges()
    {
        // Arrange
        var cmd = new ActivateUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var user = User.Create(
            cmd.TenantId,
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        user.Deactivate(_mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var cmd = new ActivateUserCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var user = User.Create(
            cmd.TenantId,
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
            .Setup(x => x.GetUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Failure(saveError));

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);
        user.IsActive.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
