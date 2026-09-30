using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class CreateRoleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IRolesRepository> _rolesRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly CreateRoleCommandHandler _sut;

    public CreateRoleCommandHandlerTests()
    {
        _unitOfWork = new();
        _rolesRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new CreateRoleCommandHandler(
            _unitOfWork.Object,
            _rolesRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenRoleNameAlreadyExists_ReturnsConflictFailure()
    {
        // Arrange
        var cmd = new CreateRoleCommand(
            Guid.CreateVersion7(),
            "Admin",
            "Administrator role",
            SystemPermission.None);

        _rolesRepository
            .Setup(x => x.ExistsByNameAsync(cmd.TenantId, RoleName.Create(cmd.Name)))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Conflict);
        result.Error.Code.Should().Be(ApplicationErrors.RoleNameAlreadyExists.Code);

        _rolesRepository.Verify(x => x.AddRole(It.IsAny<Role>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var cmd = new CreateRoleCommand(
            Guid.CreateVersion7(),
            "Admin",
            "Administrator role",
            SystemPermission.None);

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

        _rolesRepository
            .Setup(x => x.ExistsByNameAsync(cmd.TenantId, RoleName.Create(cmd.Name)))
            .ReturnsAsync(false);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Failure(saveError));

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);

        _rolesRepository.Verify(x => x.AddRole(It.Is<Role>(r =>
            r.TenantId == cmd.TenantId)), Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleIsCreatedSuccessfully_ReturnsRoleDto()
    {
        // Arrange
        var cmd = new CreateRoleCommand(
            Guid.CreateVersion7(),
            "Admin",
            "Administrator role",
            SystemPermission.None);

        Role? addedRole = null;

        _rolesRepository
            .Setup(x => x.ExistsByNameAsync(cmd.TenantId, RoleName.Create(cmd.Name)))
            .ReturnsAsync(false);

        _rolesRepository
            .Setup(x => x.AddRole(It.IsAny<Role>()))
            .Callback<Role>(role => addedRole = role);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        addedRole.Should().NotBeNull();
        addedRole!.TenantId.Should().Be(cmd.TenantId);

        result.Value.Should().BeEquivalentTo(addedRole.ToDto());

        _rolesRepository.Verify(x => x.AddRole(It.IsAny<Role>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
