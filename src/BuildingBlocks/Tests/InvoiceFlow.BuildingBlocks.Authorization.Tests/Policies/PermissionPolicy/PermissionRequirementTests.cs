using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class PermissionRequirementTests
{
    [Fact]
    public void Constructor_WithValidPermission_ShouldSetPermission()
    {
        var permission = SystemPermissions.CustomerRead;

        var requirement = new PermissionRequirement(permission);

        requirement.Permission.Should().Be(permission);
    }

    [Fact]
    public void Constructor_WithNone_ShouldThrow()
    {
        var action = () =>
            new PermissionRequirement(SystemPermissions.None);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("permission");
    }

    [Fact]
    public void Constructor_WithInvalidPermission_ShouldThrow()
    {
        var invalidPermission =
            (SystemPermissions)(1L << 62);

        var action = () =>
            new PermissionRequirement(invalidPermission);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("permission");
    }

    [Fact]
    public void Constructor_WithMultipleValidPermissions_ShouldSucceed()
    {
        var permission =
            SystemPermissions.CustomerRead |
            SystemPermissions.CustomerUpdate;

        var requirement = new PermissionRequirement(permission);

        requirement.Permission.Should().Be(permission);
    }
}
