using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public class CreatePlatformUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IPlatformUsersRepository> _usersRepository;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly CreatePlatformUserCommandHandler _sut;

    public CreatePlatformUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _passwordHasher = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new CreatePlatformUserCommandHandler(
            _unitOfWork.Object,
            _usersRepository.Object,
            _passwordHasher.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserEmailAlreadyExists_ReturnsConflictFailure()
    {
        // Arrange
        var cmd = new CreatePlatformUserCommand(
            "john.doe@test.com",
            "password",
            "John",
            "Doe");

        _usersRepository
            .Setup(x => x.ExistsByEmailAsync(UserEmail.Create(cmd.Email), CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Conflict);
        result.Error.Code.Should().Be(ApplicationErrors.UserEmailAlreadyExists.Code);

        _passwordHasher.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _usersRepository.Verify(x => x.AddUser(It.IsAny<PlatformUser>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPlatformUserIsCreatedSuccessfully_ReturnsUserDto()
    {
        // Arrange
        var ct = CancellationToken.None;

        var cmd = new CreatePlatformUserCommand(
            "john.doe@test.com",
            "password",
            "John",
            "Doe");

        PlatformUser? addedUser = null;

        _usersRepository
            .Setup(x => x.ExistsByEmailAsync(UserEmail.Create(cmd.Email), CancellationToken.None))
            .ReturnsAsync(false);

        _passwordHasher
            .Setup(x => x.HashPassword(cmd.Password))
            .Returns("password-hash");

        _usersRepository
            .Setup(x => x.AddUser(It.IsAny<PlatformUser>()))
            .Callback<PlatformUser>(user => addedUser = user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(ct))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();

        addedUser.Should().NotBeNull();
        addedUser.Email.Value.Should().Be(cmd.Email);
        addedUser.PasswordHash.Value.Should().Be("password-hash");

        result.Value.Should().BeEquivalentTo(addedUser.ToDto());

        _usersRepository.Verify(x => x.AddUser(It.IsAny<PlatformUser>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(ct), Times.Once);
    }
}
