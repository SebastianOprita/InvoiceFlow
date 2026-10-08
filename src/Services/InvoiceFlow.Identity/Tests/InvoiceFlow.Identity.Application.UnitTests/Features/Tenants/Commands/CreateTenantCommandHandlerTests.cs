using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public class CreateTenantCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ITenantsRepository> _tenantsRepository;
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly CreateTenantCommandHandler _sut;

    public CreateTenantCommandHandlerTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _tenantsRepository = new Mock<ITenantsRepository>();
        _mockDateTimeProvider = new Mock<ISystemDateTimeProvider>();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _sut = new CreateTenantCommandHandler(_unitOfWork.Object, _tenantsRepository.Object, _mockDateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ReturnsConflictFailure()
    {
        var slug = "test-tenant";

        _tenantsRepository
            .Setup(x => x.ExistsBySlugAsync(TenantSlug.Create(slug), TestContext.Current.CancellationToken))
            .ReturnsAsync(true);

        var result = await _sut.Handle(
            new CreateTenantCommand("Test Tenant", slug),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ApplicationErrorType.Conflict);
        result.Error.Code.Should().Be(ApplicationErrors.TenantSlugAlreadyExists.Code);
        result.Error.Message.Should().Be(ApplicationErrors.TenantSlugAlreadyExists.Message);
        _tenantsRepository.Verify(x => x.AddTenant(It.IsAny<Tenant>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTenantIsCreatedSuccessfully_AddsTenantSavesAndReturnsDto()
    {
        var name = "Test Tenant";
        var slug = "test-tenant";

        _tenantsRepository
            .Setup(x => x.ExistsBySlugAsync(TenantSlug.Create(slug), TestContext.Current.CancellationToken))
            .ReturnsAsync(false);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(TestContext.Current.CancellationToken))
            .ReturnsAsync(Result.Success());

        var result = await _sut.Handle(
            new CreateTenantCommand(name, slug),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Slug.Should().Be(slug);
        _tenantsRepository.Verify(
            x => x.AddTenant(It.Is<Tenant>(t =>
                t.Name.Value == name &&
                t.Slug.Value == slug)),
            Times.Once);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCalled_PassesCancellationTokenToSaveChanges()
    {
        var name = "Test Tenant";
        var slug = "test-tenant";

        using var cts = new CancellationTokenSource();

        _tenantsRepository
            .Setup(x => x.ExistsBySlugAsync(TenantSlug.Create(slug), cts.Token))
            .ReturnsAsync(false);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(cts.Token))
            .ReturnsAsync(Result.Success());

        await _sut.Handle(
            new CreateTenantCommand(name, slug),
            cts.Token);

        _unitOfWork.Verify(x => x.SaveChangesAsync(cts.Token), Times.Once);
    }
}
