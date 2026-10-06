using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public class ActivateTenantCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ITenantsRepository> _tenantsRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private ActivateTenantCommandHandler _sut;

    public ActivateTenantCommandHandlerTests()
    {
        _unitOfWork = new();
        _tenantsRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new ActivateTenantCommandHandler(_unitOfWork.Object, _tenantsRepository.Object, _mockDateTimeProvider.Object);
    }


    [Fact]
    public async Task Handle_WhenTenantDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId))
            .Returns(Task.FromResult((Tenant?)null));

        // Act
        var result = await _sut.Handle(
            new ActivateTenantCommand(tenantId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.TenantNotFound.Code);
        result.Error.Message.Should().Be(ApplicationErrors.TenantNotFound.Message);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIsAlreadyActive_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId);
        tenant.Activate(_mockDateTimeProvider.Object.Now);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        // Act
        var result = await _sut.Handle(
            new ActivateTenantCommand(tenantId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIsActive_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId);

        var expectedResult = Result.Success();

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(
            new ActivateTenantCommand(tenantId),
            CancellationToken.None);

        // Assert
        tenant.IsActive.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIsInactive_ActivatesTenantAndSavesChanges()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId);
        tenant.Deactivate(_mockDateTimeProvider.Object.Now);

        var expectedResult = Result.Success();

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(
            new ActivateTenantCommand(tenantId),
            CancellationToken.None);

        // Assert
        tenant.IsActive.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId);
        tenant.Deactivate(_mockDateTimeProvider.Object.Now);

        var expectedError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

        var expectedResult = Result.Failure(expectedError);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(
            new ActivateTenantCommand(tenantId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
        tenant.IsActive.Should().BeTrue();
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }


    private Tenant CreateTenant(Guid tenantId)
    {
        var tenant = Guid.CreateVersion7();
        return Tenant.Create(tenantId, TenantName.Create("TestTenant" + tenant), TenantSlug.Create("tenant-" + tenant), _mockDateTimeProvider.Object.Now);
    }
}
