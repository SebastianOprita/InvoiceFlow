using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public class RevokeUserRoleCommandHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IUsersRepository> _usersRepository;
    private readonly Mock<IRolesRepository> _rolesRepository;

    private RevokeUserRoleCommandHandler _sut;

    public RevokeUserRoleCommandHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _unitOfWork = new();
        _usersRepository = new();
        _rolesRepository = new();
        _sut = new(_unitOfWork.Object, _usersRepository.Object, _rolesRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new RevokeUserRoleCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

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

        _rolesRepository.Verify(
            x => x.GetRoleByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new RevokeUserRoleCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var user = User.Create(
            cmd.TenantId,
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _rolesRepository
            .Setup(x => x.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId, TestContext.Current.CancellationToken))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.RoleNotFound.Code);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserAndRoleExist_RevokesRoleAndSavesChanges()
    {
        // Arrange
        var cmd = new RevokeUserRoleCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7());

        var user = User.Create(
            cmd.TenantId,
            cmd.UserId,
            UserEmail.Create("john.doe@test.com"),
            PasswordHash.Create("hashedpassword"),
            FirstName.Create("John"),
            LastName.Create("Doe"),
            _mockDateTimeProvider.Object.Now);

        var role = Role.Create(
            cmd.TenantId,
            cmd.RoleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(SystemPermission.None));

        _usersRepository
            .Setup(x => x.GetTrackedUserByIdAsync(cmd.TenantId, cmd.UserId, TestContext.Current.CancellationToken))
            .ReturnsAsync(user);

        _rolesRepository
            .Setup(x => x.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId, TestContext.Current.CancellationToken))
            .ReturnsAsync(role);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}