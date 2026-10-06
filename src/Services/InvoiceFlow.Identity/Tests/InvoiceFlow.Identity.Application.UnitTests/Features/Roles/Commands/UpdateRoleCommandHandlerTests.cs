using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class UpdateRoleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IRolesRepository> _rolesRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private UpdateRoleCommandHandler _sut;

    public UpdateRoleCommandHandlerTests()
    {
        _unitOfWork = new();
        _rolesRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new UpdateRoleCommandHandler(
            _unitOfWork.Object,
            _rolesRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new UpdateRoleCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Admin",
            "Administrator role");

        _rolesRepository
            .Setup(x => x.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.RoleNotFound.Code);

        _rolesRepository.Verify(
            x => x.ExistsByNameAsync(It.IsAny<Guid>(), It.IsAny<RoleName>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNameIsChangedAndNewNameAlreadyExists_ReturnsConflictFailure()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var role = Role.Create(
            tenantId,
            roleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(SystemPermission.None));

        var cmd = new UpdateRoleCommand(
            tenantId,
            roleId,
            "Manager",
            "Manager role");

        _rolesRepository
            .Setup(x => x.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync(role);

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

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNameIsUnchanged_DoesNotCheckNameExists()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var role = Role.Create(
            tenantId,
            roleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Old description"),
            RolePermissions.Create(SystemPermission.None));

        var cmd = new UpdateRoleCommand(
            tenantId,
            roleId,
            "Admin",
            "New description");

        _rolesRepository
            .Setup(x => x.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync(role);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(role.ToDto());

        _rolesRepository.Verify(
            x => x.ExistsByNameAsync(It.IsAny<Guid>(), It.IsAny<RoleName>()),
            Times.Never);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var role = Role.Create(
            tenantId,
            roleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Old description"),
            RolePermissions.Create(SystemPermission.None));

        var cmd = new UpdateRoleCommand(
            tenantId,
            roleId,
            "Manager",
            "New description");

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

        _rolesRepository
            .Setup(x => x.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync(role);

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

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNameIsChangedAndNameDoesNotExist_UpdatesRoleAndReturnsRoleDto()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var roleId = Guid.CreateVersion7();

        var role = Role.Create(
            tenantId,
            roleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Old description"),
            RolePermissions.Create(SystemPermission.None));

        var cmd = new UpdateRoleCommand(
            tenantId,
            roleId,
            "Manager",
            "New description");

        _rolesRepository
            .Setup(x => x.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync(role);

        _rolesRepository
            .Setup(x => x.ExistsByNameAsync(cmd.TenantId, RoleName.Create(cmd.Name)))
            .ReturnsAsync(false);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(role.ToDto());

        _rolesRepository.Verify(
            x => x.ExistsByNameAsync(cmd.TenantId, RoleName.Create(cmd.Name)),
            Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
