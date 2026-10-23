using FluentAssertions;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Roles;

public sealed class RoleMapperTests
{
    [Fact]
    public void ToDto_ShouldMapRoleToRoleDto()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);

        var role = CreateRole(createdAtUtc: createdAtUtc);

        // Act
        var dto = role.ToDto();

        // Assert
        dto.TenantId.Should().Be(role.TenantId);
        dto.Id.Should().Be(role.Id);
        dto.Name.Should().Be(role.Name.Value);
        dto.Description.Should().Be(role.Description!.Value);
        dto.Permissions.Should().Be(role.Permissions.Value);
        dto.CreatedAtUtc.Should().Be(createdAtUtc);
        dto.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ToDto_ShouldMapDescriptionAsNull_WhenRoleDescriptionIsNull()
    {
        // Arrange
        var role = CreateRole(withDescription: false);

        // Act
        var dto = role.ToDto();

        // Assert
        dto.Description.Should().BeNull();
    }

    [Fact]
    public void ToDto_ShouldMapUpdatedAtUtc_WhenRoleWasUpdated()
    {
        // Arrange
        var createdAtUtc = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var updatedAtUtc = new DateTimeOffset(2024, 1, 2, 10, 0, 0, TimeSpan.Zero);

        var role = CreateRole(createdAtUtc: createdAtUtc);

        role.UpdateDetails(
            RoleName.Create("Admin Updated"),
            updatedAtUtc,
            RoleDescription.Create("Updated description"));

        // Act
        var dto = role.ToDto();

        // Assert
        dto.Name.Should().Be("Admin Updated");
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    private static Role CreateRole(
        DateTimeOffset? createdAtUtc = null,
        bool withDescription = true)
    {
        return Role.Create(
            id: Guid.CreateVersion7(),
            tenantId: Guid.CreateVersion7(),
            name: RoleName.Create("Admin"),
            description: withDescription
                ? RoleDescription.Create("Administrator role")
                : null,
            permissions: RolePermissions.Create(1L << 3),
            createdAtUtc: createdAtUtc ?? new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero));
    }
}
