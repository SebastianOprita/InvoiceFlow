using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public class DeactivateTenantCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ITenantsRepository> _tenantsRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly DeactivateTenantCommandHandler _sut;

    public DeactivateTenantCommandHandlerTests()
    {
        _unitOfWork = new();
        _tenantsRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new DeactivateTenantCommandHandler(
            _unitOfWork.Object,
            _tenantsRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenTenantDoesNotExist_ReturnsNotFoundFailure()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        _tenantsRepository
            .Setup(x => x.GetTrackedTenantByIdAsync(tenantId, TestContext.Current.CancellationToken))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _sut.Handle(
            new DeactivateTenantCommand(tenantId),
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
    public async Task Handle_WhenTenantIsAlreadyInactive_ReturnsSuccessWithoutSaving()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId);
        tenant.Deactivate(_mockDateTimeProvider.Object.Now);

        _tenantsRepository
            .Setup(x => x.GetTrackedTenantByIdAsync(tenantId, TestContext.Current.CancellationToken))
            .ReturnsAsync(tenant);

        // Act
        var result = await _sut.Handle(
            new DeactivateTenantCommand(tenantId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tenant.IsActive.Should().BeFalse();
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIsActive_DeactivatesTenantAndSavesChanges()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId);

        _tenantsRepository
            .Setup(x => x.GetTrackedTenantByIdAsync(tenantId, TestContext.Current.CancellationToken))
            .ReturnsAsync(tenant);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.Handle(
            new DeactivateTenantCommand(tenantId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tenant.IsActive.Should().BeFalse();
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private Tenant CreateTenant(Guid tenantId)
    {
        return Tenant.Create(tenantId, TenantName.Create("Test Tenant"), TenantSlug.Create("test-tenant"), _mockDateTimeProvider.Object.Now);
    }
}
