using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using Moq;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public sealed class GetTenantsQueryHandlerTests
{
    private readonly Mock<ISystemDateTimeProvider> _mockDateTimeProvider;
    private readonly Mock<ITenantsRepository> _tenantsRepositoryMock = new();
    private readonly GetTenantsQueryHandler _handler;

    public GetTenantsQueryHandlerTests()
    {
        _mockDateTimeProvider = new();
        _mockDateTimeProvider.Setup(x => x.Now).Returns(DateTime.UtcNow);
        _handler = new GetTenantsQueryHandler(_tenantsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTenantsExist_ShouldReturnMappedTenantDtos()
    {
        // Arrange
        var tenants = new List<Tenant>
        {
            Tenant.Create(
                Guid.CreateVersion7(),
                TenantName.Create("tenant1"),
                TenantSlug.Create("tenant1"),
                _mockDateTimeProvider.Object.Now),
            Tenant.Create(
                Guid.CreateVersion7(),
                TenantName.Create("tenant2"),
                TenantSlug.Create("tenant2"),
                _mockDateTimeProvider.Object.Now),
        };

        var query = new GetTenantsQuery();

        _tenantsRepositoryMock
            .Setup(x => x.GetAllTenantsAsync())
            .ReturnsAsync(tenants);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(
            tenants.Select(t => t.ToDto()));

        _tenantsRepositoryMock.Verify(
            x => x.GetAllTenantsAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoTenantsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetTenantsQuery();

        _tenantsRepositoryMock
            .Setup(x => x.GetAllTenantsAsync())
            .ReturnsAsync(new List<Tenant>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _tenantsRepositoryMock.Verify(
            x => x.GetAllTenantsAsync(),
            Times.Once);
    }
}
