using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Application.Features;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _unitOfWork = new();
        _usersRepository = new();
        _passwordHasher = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new CreateUserCommandHandler(
            _unitOfWork.Object,
            _usersRepository.Object,
            _passwordHasher.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenUserEmailAlreadyExists_ReturnsConflictFailure()
    {
        // Arrange
        var cmd = new CreateUserCommand(
            Guid.CreateVersion7(),
            "john.doe@test.com",
            "password",
            "John",
            "Doe");

        _usersRepository
            .Setup(x => x.ExistsByEmailAsync(cmd.TenantId, UserEmail.Create(cmd.Email), TestContext.Current.CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Conflict);
        result.Error.Code.Should().Be(ApplicationErrors.UserEmailAlreadyExists.Code);

        _passwordHasher.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _usersRepository.Verify(x => x.AddUser(It.IsAny<User>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var cmd = new CreateUserCommand(
            Guid.CreateVersion7(),
            "john.doe@test.com",
            "password",
            "John",
            "Doe");

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

        _usersRepository
            .Setup(x => x.ExistsByEmailAsync(cmd.TenantId, UserEmail.Create(cmd.Email), TestContext.Current.CancellationToken))
            .ReturnsAsync(false);

        _passwordHasher
            .Setup(x => x.HashPassword(cmd.Password))
            .Returns("password-hash");

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Failure(saveError));

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);

        _usersRepository.Verify(x => x.AddUser(It.Is<User>(u =>
            u.TenantId == cmd.TenantId &&
            u.Email.Value == cmd.Email &&
            u.PasswordHash.Value == "password-hash")), Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsCreatedSuccessfully_ReturnsUserDto()
    {
        // Arrange
        var cmd = new CreateUserCommand(
            Guid.CreateVersion7(),
            "john.doe@test.com",
            "password",
            "John",
            "Doe");

        User? addedUser = null;

        _usersRepository
            .Setup(x => x.ExistsByEmailAsync(cmd.TenantId, UserEmail.Create(cmd.Email), TestContext.Current.CancellationToken))
            .ReturnsAsync(false);

        _passwordHasher
            .Setup(x => x.HashPassword(cmd.Password))
            .Returns("password-hash");

        _usersRepository
            .Setup(x => x.AddUser(It.IsAny<User>()))
            .Callback<User>(user => addedUser = user);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        addedUser.Should().NotBeNull();
        addedUser.TenantId.Should().NotBeEmpty();
        addedUser!.TenantId.Should().Be(cmd.TenantId);
        addedUser.Email.Value.Should().Be(cmd.Email);
        addedUser.PasswordHash.Value.Should().Be("password-hash");

        result.Value.Should().BeEquivalentTo(addedUser.ToDto());

        _usersRepository.Verify(x => x.AddUser(It.IsAny<User>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
