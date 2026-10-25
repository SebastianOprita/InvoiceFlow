using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public class ActivateRoleCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Mock<ISystemDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRolesRepository> _rolesRepository = new();
    private readonly ActivateRoleCommandHandler _sut;

    public ActivateRoleCommandHandlerTests()
    {
        _dateTimeProvider.Setup(x => x.GetUtcNow()).Returns(Now);
        _sut = new ActivateRoleCommandHandler(
            _unitOfWork.Object,
            _rolesRepository.Object,
            _dateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenRoleDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        var command = new ActivateRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());
        _rolesRepository
            .Setup(x => x.GetTrackedRoleByIdAsync(command.TenantId, command.RoleId))
            .ReturnsAsync((Role?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.RoleNotFound.Code);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleIsAlreadyActive_ReturnsSuccessWithoutSaving()
    {
        var command = new ActivateRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());
        var role = CreateRole(command);
        _rolesRepository
            .Setup(x => x.GetTrackedRoleByIdAsync(command.TenantId, command.RoleId))
            .ReturnsAsync(role);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        role.IsActive.Should().BeTrue();
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleIsInactive_ActivatesRoleAndSavesChanges()
    {
        var command = new ActivateRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());
        var role = CreateRole(command);
        role.Deactivate(Now);
        _rolesRepository
            .Setup(x => x.GetTrackedRoleByIdAsync(command.TenantId, command.RoleId))
            .ReturnsAsync(role);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        role.IsActive.Should().BeTrue();
        role.UpdatedAtUtc.Should().Be(Now);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        var command = new ActivateRoleCommand(Guid.CreateVersion7(), Guid.CreateVersion7());
        var role = Role.Create(
            command.TenantId,
            command.RoleId,
            RoleName.Create("Admin"),
            Now.AddMinutes(-2));
        role.Deactivate(Now.AddMinutes(-1));
        var saveError = new ApplicationError(ApplicationErrorType.Validation, "save.failed", "Save failed.");
        _rolesRepository
            .Setup(x => x.GetTrackedRoleByIdAsync(command.TenantId, command.RoleId))
            .ReturnsAsync(role);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(saveError));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);
        role.IsActive.Should().BeTrue();
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Role CreateRole(ActivateRoleCommand command)
    {
        return Role.Create(
            command.TenantId,
            command.RoleId,
            RoleName.Create("Admin"),
            Now);
    }
}
