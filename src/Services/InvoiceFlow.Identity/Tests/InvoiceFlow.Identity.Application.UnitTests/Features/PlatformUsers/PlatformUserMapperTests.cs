using FluentAssertions;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.PlatformUsers;

public sealed class PlatformUserMapperTests
{
    [Fact]
    public void ToDto_ShouldMapPlatformUserToUserDto()
    {
        // Arrange
        var createdAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var platformUser = CreatePlatformUser(createdAtUtc: createdAtUtc);

        // Act
        var dto = platformUser.ToDto();

        // Assert
        dto.Id.Should().Be(platformUser.Id);
        dto.Email.Should().Be(platformUser.Email.Value);
        dto.FirstName.Should().Be(platformUser.FirstName.Value);
        dto.LastName.Should().Be(platformUser.LastName.Value);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAtUtc.Should().Be(createdAtUtc);
        dto.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ToDto_ShouldMapUpdatedAtUtc_WhenPlatformUserWasUpdated()
    {
        // Arrange
        var createdAtUtc = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAtUtc = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc);

        var platformUser = CreatePlatformUser(createdAtUtc: createdAtUtc);

        platformUser.UpdateProfile(
            FirstName.Create("Updated"),
            LastName.Create("User"),
            updatedAtUtc);

        // Act
        var dto = platformUser.ToDto();

        // Assert
        dto.FirstName.Should().Be("Updated");
        dto.LastName.Should().Be("User");
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void ToDto_ShouldMapInactivePlatformUser()
    {
        // Arrange
        var platformUser = CreatePlatformUser();
        var updatedAtUtc = platformUser.CreatedAtUtc.AddDays(1);

        platformUser.Deactivate(updatedAtUtc);

        // Act
        var dto = platformUser.ToDto();

        // Assert
        dto.IsActive.Should().BeFalse();
        dto.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    private static Domain.PlatformUser CreatePlatformUser(DateTime? createdAtUtc = null)
    {
        return Domain.PlatformUser.Create(
            id: Guid.CreateVersion7(),
            email: UserEmail.Create("john.doe@test.com"),
            passwordHash: PasswordHash.Create("hashed-password"),
            firstName: FirstName.Create("John"),
            lastName: LastName.Create("Doe"),
            createdAtUtc: createdAtUtc ?? new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc));
    }
}
