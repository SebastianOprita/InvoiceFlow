using FluentAssertions;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Tenants;

public sealed class TenantMapperTests
{
    [Fact]
    public void ToDto_ShouldMapTenantToTenantDto()
    {
        // Arrange
        var createdAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var tenant = CreateTenant(createdAtUtc: createdAtUtc);

        // Act
        var dto = tenant.ToDto();

        // Assert
        dto.Id.Should().Be(tenant.Id);
        dto.Name.Should().Be(tenant.Name.Value);
        dto.Slug.Should().Be(tenant.Slug.Value);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAtUtc.Should().Be(createdAtUtc);
        dto.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ToDto_ShouldMapUpdatedAtUtc_WhenTenantWasUpdated()
    {
        // Arrange
        var createdAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAtUtc = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc);

        var tenant = CreateTenant(createdAtUtc: createdAtUtc);

        tenant.UpdateName(
            TenantName.Create("Updated Tenant"),
            updatedAtUtc);

        // Act
        var dto = tenant.ToDto();

        // Assert
        dto.Name.Should().Be("Updated Tenant");
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void ToDto_ShouldMapInactiveTenant()
    {
        // Arrange
        var tenant = CreateTenant();
        var updatedAtUtc = tenant.CreatedAtUtc.AddDays(1);

        tenant.Deactivate(updatedAtUtc);

        // Act
        var dto = tenant.ToDto();

        // Assert
        dto.IsActive.Should().BeFalse();
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    private static Tenant CreateTenant(DateTime? createdAtUtc = null)
    {
        return Tenant.Create(
            id: Guid.CreateVersion7(),
            name: TenantName.Create("Test Tenant"),
            slug: TenantSlug.Create("test-tenant"),
            createdAtUtc: createdAtUtc ?? new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc));
    }
}
