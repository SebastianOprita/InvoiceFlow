using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class RolePermissionsTests
{
    private const long PermissionA = 1L << 3; // CustomerRead
    private const long PermissionB = 1L << 4; // CustomerCreate
    private const long InvalidPermission = 1L << 13;


    [Fact]
    public void None_ShouldContainNoPermissions()
    {
        RolePermissions.None.Value.Should().Be(RolePermissions.None.Value);
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
        var act = () => RolePermissions.Create(InvalidPermission);

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

        var act = () => permissions.Grant(RolePermissions.None.Value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionRequired);
    }

    [Fact]
    public void Grant_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = RolePermissions.None;
        var invalidPermission = InvalidPermission;

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

        var act = () => permissions.Revoke(RolePermissions.None.Value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionRequired);
    }

    [Fact]
    public void Revoke_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = RolePermissions.Create(PermissionA);
        var invalidPermission = InvalidPermission;

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

        var result = permissions.Has(RolePermissions.None.Value);

        result.Should().BeFalse();
    }

    [Fact]
    public void Has_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = RolePermissions.Create(PermissionA);
        var invalidPermission = InvalidPermission;

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
