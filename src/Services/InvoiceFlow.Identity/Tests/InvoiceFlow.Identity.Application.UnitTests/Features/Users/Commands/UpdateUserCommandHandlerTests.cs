using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private UpdateUserCommandHandler _sut;

    public UpdateUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new UpdateUserCommandHandler(
            _unitOfWork.Object,
            _usersRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new UpdateUserCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "John",
            "Doe");

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

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserExists_UpdatesProfileAndReturnsUserDto()
    {
        // Arrange
        var cmd = new UpdateUserCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "John",
            "Doe");

        var user = User.Create(
            cmd.TenantId,
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("password-hash"),
            FirstName.Create("Old"),
            LastName.Create("Name"),
            _mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        user.FirstName.Value.Should().Be(cmd.FirstName);
        user.LastName.Value.Should().Be(cmd.LastName);

        result.Value.Should().BeEquivalentTo(user.ToDto());

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}