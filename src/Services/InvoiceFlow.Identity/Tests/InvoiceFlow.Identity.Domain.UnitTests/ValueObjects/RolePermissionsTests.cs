using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class RolePermissionsTests
{
    private const SystemPermission PermissionA = SystemPermission.CustomerView;
    private const SystemPermission PermissionB = SystemPermission.CustomerCreate;

    [Fact]
    public void None_ShouldContainNoPermissions()
    {
        RolePermissions.None.Value.Should().Be(SystemPermission.None);
    }

    [Fact]
    public void Create_WithValidPermissions_ShouldCreateRolePermissions()
    {
        var permissions = RolePermissions.Create(PermissionA);

        permissions.Value.Should().Be(PermissionA);
    }

    [Fact]
    public void Create_WithInvalidPermissions_ShouldThrowDomainException()
    {
        var invalidPermission = (SystemPermission)int.MaxValue;

        var act = () => RolePermissions.Create(invalidPermission);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionInvalid);
    }

    [Fact]
    public void Grant_WithValidPermission_ShouldAddPermission()
    {
        var permissions = RolePermissions.None;

        var updated = permissions.Grant(PermissionA);

        updated.Has(PermissionA).Should().BeTrue();
    }

    [Fact]
    public void Grant_WithNonePermission_ShouldThrowDomainException()
    {
        var permissions = RolePermissions.None;

        var act = () => permissions.Grant(SystemPermission.None);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionRequired);
    }

    [Fact]
    public void Grant_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = RolePermissions.None;
        var invalidPermission = (SystemPermission)int.MaxValue;

        var act = () => permissions.Grant(invalidPermission);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionInvalid);
    }

    [Fact]
    public void Revoke_WithValidPermission_ShouldRemovePermission()
    {
        var permissions = RolePermissions.Create(PermissionA | PermissionB);

        var updated = permissions.Revoke(PermissionA);

        updated.Has(PermissionA).Should().BeFalse();
        updated.Has(PermissionB).Should().BeTrue();
    }

    [Fact]
    public void Revoke_WithNonePermission_ShouldThrowDomainException()
    {
        var permissions = RolePermissions.Create(PermissionA);

        var act = () => permissions.Revoke(SystemPermission.None);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionRequired);
    }

    [Fact]
    public void Revoke_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = RolePermissions.Create(PermissionA);
        var invalidPermission = (SystemPermission)int.MaxValue;

        var act = () => permissions.Revoke(invalidPermission);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionInvalid);
    }

    [Fact]
    public void Has_WithExistingPermission_ShouldReturnTrue()
    {
        var permissions = RolePermissions.Create(PermissionA | PermissionB);

        var result = permissions.Has(PermissionA);

        result.Should().BeTrue();
    }

    [Fact]
    public void Has_WithMissingPermission_ShouldReturnFalse()
    {
        var permissions = RolePermissions.Create(PermissionA);

        var result = permissions.Has(PermissionB);

        result.Should().BeFalse();
    }

    [Fact]
    public void Has_WithNonePermission_ShouldReturnFalse()
    {
        var permissions = RolePermissions.Create(PermissionA);

        var result = permissions.Has(SystemPermission.None);

        result.Should().BeFalse();
    }

    [Fact]
    public void Has_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = RolePermissions.Create(PermissionA);
        var invalidPermission = (SystemPermission)int.MaxValue;

        var act = () => permissions.Has(invalidPermission);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionInvalid);
    }

    [Fact]
    public void ToString_ShouldReturnPermissionsAsString()
    {
        var permissions = RolePermissions.Create(PermissionA);

        permissions.ToString().Should().Be(PermissionA.ToString());
    }
}
