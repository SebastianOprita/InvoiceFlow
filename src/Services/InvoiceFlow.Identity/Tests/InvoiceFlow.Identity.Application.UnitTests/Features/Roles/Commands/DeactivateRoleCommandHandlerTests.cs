using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class DeactivateRoleCommandHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IRolesRepository> _rolesRepository;

    private DeactivateRoleCommandHandler _sut;

    public DeactivateRoleCommandHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _unitOfWork = new();
        _rolesRepository = new();
        _sut = new DeactivateRoleCommandHandler(_unitOfWork.Object, _rolesRepository.Object, _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var cmd = new DeactivateRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        _rolesRepository
            .Setup(x => x.GetTrackedRoleByIdAsync(cmd.TenantId, cmd.RoleId))
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
    public async Task Handle_WhenRoleExists_DeactivatesRoleAndSavesChanges()
    {
        // Arrange
        var cmd = new DeactivateRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var role = Role.Create(
            cmd.TenantId,
            cmd.RoleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.GetUtcNow(),
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(RolePermissions.None.Value));

        _rolesRepository
            .Setup(x => x.GetTrackedRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync(role);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.IsActive.Should().BeFalse();
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleIsAlreadyInactive_ReturnsSuccessWithoutSaving()
    {
        var cmd = new DeactivateRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());
        var role = Role.Create(
            cmd.TenantId,
            cmd.RoleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.GetUtcNow());
        role.Deactivate(_mockDateTimeProvider.Object.GetUtcNow());

        _rolesRepository
            .Setup(x => x.GetTrackedRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync(role);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        role.IsActive.Should().BeFalse();
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var cmd = new DeactivateRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        var role = Role.Create(
            cmd.TenantId,
            cmd.RoleId,
            RoleName.Create("Admin"),
            _mockDateTimeProvider.Object.GetUtcNow(),
            RoleDescription.CreateOptional("Administrator role"),
            RolePermissions.Create(RolePermissions.None.Value));

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            "save.failed",
            "Save failed.");

        _rolesRepository
            .Setup(x => x.GetTrackedRoleByIdAsync(cmd.TenantId, cmd.RoleId))
            .ReturnsAsync(role);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(Result.Failure(saveError));

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);

        role.IsActive.Should().BeFalse();
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
