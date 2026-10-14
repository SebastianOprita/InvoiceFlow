using FluentAssertions;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class SystemPermissionsExtensionsTests
{
    [Fact]
    public void IsValid_WithNone_ShouldReturnTrue()
    {
        SystemPermissions.None
            .IsValid()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsValid_WithSingleValidPermission_ShouldReturnTrue()
    {
        SystemPermissions.CustomerRead
            .IsValid()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsValid_WithMultipleValidPermissions_ShouldReturnTrue()
    {
        var permissions =
            SystemPermissions.CustomerRead |
            SystemPermissions.CustomerCreate |
            SystemPermissions.CustomerUpdate;

        permissions
            .IsValid()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsValid_WithAllValidPermissions_ShouldReturnTrue()
    {
        SystemPermissionsExtensions.AllValidPermissions
            .IsValid()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsValid_WithUnknownPermissionBit_ShouldReturnFalse()
    {
        var unknownPermission = (SystemPermissions)(1L << 62);

        unknownPermission
            .IsValid()
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsValid_WithValidAndUnknownPermissionBits_ShouldReturnFalse()
    {
        var permissions =
            SystemPermissions.CustomerRead |
            (SystemPermissions)(1L << 62);

        permissions
            .IsValid()
            .Should()
            .BeFalse();
    }

    [Fact]
    public void AllValidPermissions_ShouldContainEveryDefinedPermission()
    {
        var definedPermissions = Enum.GetValues<SystemPermissions>();

        foreach (var permission in definedPermissions)
        {
            (SystemPermissionsExtensions.AllValidPermissions & permission)
                .Should()
                .Be(permission);
        }
    }
}
