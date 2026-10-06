using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public class UpdateTenantCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ITenantsRepository> _tenantsRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly UpdateTenantCommandHandler _sut;

    public UpdateTenantCommandHandlerTests()
    {
        _unitOfWork = new();
        _tenantsRepository = new();
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new UpdateTenantCommandHandler(
            _unitOfWork.Object,
            _tenantsRepository.Object,
            _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenTenantDoesNotExist_ReturnsNotFoundFailure()
    {
        var tenantId = Guid.CreateVersion7();

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId, TestContext.Current.CancellationToken))
            .ReturnsAsync((Tenant?)null);

        var result = await _sut.Handle(
            new UpdateTenantCommand(tenantId, "New name"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.NotFound);
        result.Error.Code.Should().Be(ApplicationErrors.TenantNotFound.Code);
        result.Error.Message.Should().Be(ApplicationErrors.TenantNotFound.Message);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsFailure()
    {
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId, TenantName.Create("Old name"));

        var saveError = new ApplicationError(
            ApplicationErrorType.Validation,
            ApplicationErrors.DbSaveFailed.Code,
            ApplicationErrors.DbSaveFailed.Message);

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId, TestContext.Current.CancellationToken))
            .ReturnsAsync(tenant);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(saveError));

        var result = await _sut.Handle(
            new UpdateTenantCommand(tenantId, "New name"),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(saveError);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTenantExists_UpdatesNameSavesAndReturnsDto()
    {
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId, TenantName.Create("Old name"));

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId, TestContext.Current.CancellationToken))
            .ReturnsAsync(tenant);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(
            new UpdateTenantCommand(tenantId, "New name"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(tenantId);
        result.Value.Name.Should().Be("New name");
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCalled_PassesCancellationTokenToSaveChanges()
    {
        var tenantId = Guid.CreateVersion7();
        var tenant = CreateTenant(tenantId, TenantName.Create("Old name"));

        using var cts = new CancellationTokenSource();

        _tenantsRepository
            .Setup(x => x.GetTenantByIdAsync(tenantId, cts.Token))
            .ReturnsAsync(tenant);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(cts.Token))
            .ReturnsAsync(Result.Success());

        await _sut.Handle(
            new UpdateTenantCommand(tenantId, "New name"),
            cts.Token);

        _unitOfWork.Verify(x => x.SaveChangesAsync(cts.Token), Times.Once);
    }

    private Tenant CreateTenant(Guid tenantId, TenantName name)
    {
        return Tenant.Create(tenantId, name, TenantSlug.Create("new-slug"), _mockDateTimeProvider.Object.Now);
    }
}
