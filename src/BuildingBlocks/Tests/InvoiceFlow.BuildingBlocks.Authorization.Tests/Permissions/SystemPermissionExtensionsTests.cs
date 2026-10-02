using FluentAssertions;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class SystemPermissionExtensionsTests
{
    [Fact]
    public void IsValid_WithNone_ShouldReturnTrue()
    {
        SystemPermission.None
            .IsValid()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsValid_WithSingleValidPermission_ShouldReturnTrue()
    {
        SystemPermission.CustomerView
            .IsValid()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsValid_WithMultipleValidPermissions_ShouldReturnTrue()
    {
        var permissions =
            SystemPermission.CustomerView |
            SystemPermission.CustomerCreate |
            SystemPermission.CustomerUpdate;

        permissions
            .IsValid()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsValid_WithAllValidPermissions_ShouldReturnTrue()
    {
        SystemPermissionExtensions.AllValidPermissions
            .IsValid()
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsValid_WithUnknownPermissionBit_ShouldReturnFalse()
    {
        var unknownPermission = (SystemPermission)(1L << 62);

        unknownPermission
            .IsValid()
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsValid_WithValidAndUnknownPermissionBits_ShouldReturnFalse()
    {
        var permissions =
            SystemPermission.CustomerView |
            (SystemPermission)(1L << 62);

        permissions
            .IsValid()
            .Should()
            .BeFalse();
    }

    [Fact]
    public void AllValidPermissions_ShouldContainEveryDefinedPermission()
    {
        var definedPermissions = Enum.GetValues<SystemPermission>();

        foreach (var permission in definedPermissions)
        {
            (SystemPermissionExtensions.AllValidPermissions & permission)
                .Should()
                .Be(permission);
        }
    }
}
