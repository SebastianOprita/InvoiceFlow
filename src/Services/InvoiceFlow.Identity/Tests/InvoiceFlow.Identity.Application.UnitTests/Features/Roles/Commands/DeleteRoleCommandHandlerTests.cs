using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class DeleteRoleCommandHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IRolesRepository> _rolesRepository;

    private DeleteRoleCommandHandler _sut;

    public DeleteRoleCommandHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _unitOfWork = new();
        _rolesRepository = new();
        _sut = new DeleteRoleCommandHandler(_unitOfWork.Object, _rolesRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new DeleteRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

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

        _rolesRepository.Verify(x => x.RemoveRole(It.IsAny<Role>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleExists_RemovesRoleAndSavesChanges()
    {
        // Arrange
        var cmd = new DeleteRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var role = Role.Create(
            cmd.TenantId,
            cmd.RoleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(SystemPermission.None));

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

        _rolesRepository.Verify(x => x.RemoveRole(role), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var cmd = new DeleteRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var role = Role.Create(
            cmd.TenantId,
            cmd.RoleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(SystemPermission.None));

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            "save.failed",
            "Save failed.");

        _rolesRepository
            .Setup(x => x.GetRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync(role);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Failure(saveError));

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);

        _rolesRepository.Verify(x => x.RemoveRole(role), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
