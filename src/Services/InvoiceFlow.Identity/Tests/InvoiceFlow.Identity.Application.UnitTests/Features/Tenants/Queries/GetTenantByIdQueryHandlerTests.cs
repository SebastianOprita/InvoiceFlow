using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public class GetTenantByIdQueryHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<ITenantsRepository> _tenantsRepositoryMock = new();
    private readonly GetTenantByIdQueryHandler _handler;

    public GetTenantByIdQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _handler = new GetTenantByIdQueryHandler(_tenantsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTenantExists_ShouldReturnSuccessResultWithTenantDto()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var tenant = Tenant.Create
        (
            tenantId,
            TenantName.Create("tenant1"),
            TenantSlug.Create("tenant1"),
            _mockDateTimeProvider.Object.Now);

        var query = new GetTenantByIdQuery(tenantId);

        _tenantsRepositoryMock
            .Setup(x => x.FindTenantByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(tenant.ToDto());

        _tenantsRepositoryMock.Verify(
            x => x.FindTenantByIdAsync(tenantId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTenantDoesNotExist_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var query = new GetTenantByIdQuery(tenantId);

        _tenantsRepositoryMock
            .Setup(x => x.FindTenantByIdAsync(tenantId))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        result.Error.Should().BeEquivalentTo(new ApplicationError(
            ApplicationErrorType.NotFound,
            ApplicationErrors.TenantNotFound.Code,
            ApplicationErrors.TenantNotFound.Message));

        _tenantsRepositoryMock.Verify(
            x => x.FindTenantByIdAsync(tenantId),
            Times.Once);
    }
}
