using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class RevokePermissionCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IRolesRepository> _rolesRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;

    private readonly RevokePermissionCommandHandler _sut;

    public RevokePermissionCommandHandlerTests()
    {
        _unitOfWork = new();
        _rolesRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new RevokePermissionCommandHandler(
            _unitOfWork.Object,
            _rolesRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new RevokePermissionCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            SystemPermission.None);

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

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var permission = SystemPermission.CustomerView;

        var cmd = new RevokePermissionCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            permission);

        var role = Role.Create(
            cmd.TenantId,
            cmd.RoleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(permission));

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

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

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleExists_RevokesPermissionAndReturnsRoleDto()
    {
        // Arrange
        var permission = SystemPermission.CustomerView;

        var cmd = new RevokePermissionCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            permission);

        var role = Role.Create(
            cmd.TenantId,
            cmd.RoleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.Now,
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(permission));

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

        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
