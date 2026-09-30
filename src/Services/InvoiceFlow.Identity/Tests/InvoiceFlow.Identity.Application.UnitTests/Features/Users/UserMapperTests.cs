using FluentAssertions;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.Users;

public sealed class UserMapperTests
{
    [Fact]
    public void ToDto_ShouldMapUserToUserDto()
    {
        // Arrange
        var createdAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var user = CreateUser(createdAtUtc: createdAtUtc);

        // Act
        var dto = user.ToDto();

        // Assert
        dto.TenantId.Should().Be(user.TenantId);
        dto.Id.Should().Be(user.Id);
        dto.Email.Should().Be(user.Email.Value);
        dto.FirstName.Should().Be(user.FirstName.Value);
        dto.LastName.Should().Be(user.LastName.Value);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAtUtc.Should().Be(createdAtUtc);
        dto.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ToDto_ShouldMapUpdatedAtUtc_WhenUserWasUpdated()
    {
        // Arrange
        var createdAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAtUtc = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc);

        var user = CreateUser(createdAtUtc: createdAtUtc);

        user.UpdateProfile(
            FirstName.Create("Updated"),
            LastName.Create("User"),
            updatedAtUtc);

        // Act
        var dto = user.ToDto();

        // Assert
        dto.FirstName.Should().Be("Updated");
        dto.LastName.Should().Be("User");
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void ToDto_ShouldMapInactiveUser()
    {
        // Arrange
        var user = CreateUser();
        var updatedAtUtc = user.CreatedAtUtc.AddDays(1);

        user.Deactivate(updatedAtUtc);

        // Act
        var dto = user.ToDto();

        // Assert
        dto.IsActive.Should().BeFalse();
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    private static User CreateUser(DateTime? createdAtUtc = null)
    {
        return User.Create(
            tenantId: Guid.CreateVersion7(),
            id: Guid.CreateVersion7(),
            email: UserEmail.Create("john.doe@test.com"),
            passwordHash: PasswordHash.Create("hashed-password"),
            firstName: FirstName.Create("John"),
            lastName: LastName.Create("Doe"),
            createdAtUtc: createdAtUtc ?? new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc));
    }
}
