using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class UpdatePlatformUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPlatformUsersRepository> _usersRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly UpdatePlatformUserCommandHandler _sut;

    public UpdatePlatformUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new UpdatePlatformUserCommandHandler(
            _unitOfWork.Object,
            _usersRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new UpdatePlatformUserCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe");

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

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserExists_UpdatesProfileAndReturnsUserDto()
    {
        // Arrange
        var ct = CancellationToken.None;

        var cmd = new UpdatePlatformUserCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe");

        var user = PlatformUser.Create(
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("password-hash"),
            FirstName.Create("Old"),
            LastName.Create("Name"),
            _mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.UserId, CancellationToken.None))
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(ct))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();

        user.FirstName.Value.Should().Be(cmd.FirstName);
        user.LastName.Value.Should().Be(cmd.LastName);

        result.Value.Should().BeEquivalentTo(user.ToDto());

        _unitOfWork.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }
}
