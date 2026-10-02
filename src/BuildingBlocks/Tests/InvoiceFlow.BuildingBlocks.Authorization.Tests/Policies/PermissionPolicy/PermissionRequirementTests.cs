using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class PermissionRequirementTests
{
    [Fact]
    public void Constructor_WithValidPermission_ShouldSetPermission()
    {
        var permission = SystemPermission.CustomerView;

        var requirement = new PermissionRequirement(permission);

        requirement.Permission.Should().Be(permission);
    }

    [Fact]
    public void Constructor_WithNone_ShouldThrow()
    {
        var action = () =>
            new PermissionRequirement(SystemPermission.None);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("permission");
    }

    [Fact]
    public void Constructor_WithInvalidPermission_ShouldThrow()
    {
        var invalidPermission =
            (SystemPermission)(1L << 62);

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
            SystemPermission.CustomerView |
            SystemPermission.CustomerUpdate;

        var requirement = new PermissionRequirement(permission);

        requirement.Permission.Should().Be(permission);
    }
}
