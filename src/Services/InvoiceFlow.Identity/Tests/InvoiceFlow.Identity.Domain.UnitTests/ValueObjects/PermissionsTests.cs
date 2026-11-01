using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class PermissionsTests
{
    private const long PermissionA = 1L << 3; // CustomerRead
    private const long PermissionB = 1L << 4; // CustomerCreate
    private const long InvalidPermission = 1L << 31;


    [Fact]
    public void None_ShouldContainNoPermissions()
    {
        Permissions.None.Value.Should().Be(Permissions.None.Value);
    }

    [Fact]
    public void Create_WithValidPermissions_ShouldCreatePermissions()
    {
        var permissions = Permissions.Create(PermissionA);

        permissions.Value.Should().Be(PermissionA);
    }

    [Fact]
    public void Create_WithInvalidPermissions_ShouldThrowDomainException()
    {
        var act = () => Permissions.Create(InvalidPermission);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionInvalid);
    }

    [Fact]
    public void Grant_WithValidPermission_ShouldAddPermission()
    {
        var permissions = Permissions.None;

        var updated = permissions.Grant(PermissionA);

        updated.Has(PermissionA).Should().BeTrue();
    }

    [Fact]
    public void Grant_WithNonePermission_ShouldThrowDomainException()
    {
        var permissions = Permissions.None;

        var act = () => permissions.Grant(Permissions.None.Value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionRequired);
    }

    [Fact]
    public void Grant_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = Permissions.None;
        var invalidPermission = InvalidPermission;

        var act = () => permissions.Grant(invalidPermission);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionInvalid);
    }

    [Fact]
    public void Revoke_WithValidPermission_ShouldRemovePermission()
    {
        var permissions = Permissions.Create(PermissionA | PermissionB);

        var updated = permissions.Revoke(PermissionA);

        updated.Has(PermissionA).Should().BeFalse();
        updated.Has(PermissionB).Should().BeTrue();
    }

    [Fact]
    public void Revoke_WithNonePermission_ShouldThrowDomainException()
    {
        var permissions = Permissions.Create(PermissionA);

        var act = () => permissions.Revoke(Permissions.None.Value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionRequired);
    }

    [Fact]
    public void Revoke_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = Permissions.Create(PermissionA);
        var invalidPermission = InvalidPermission;

        var act = () => permissions.Revoke(invalidPermission);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionInvalid);
    }

    [Fact]
    public void Has_WithExistingPermission_ShouldReturnTrue()
    {
        var permissions = Permissions.Create(PermissionA | PermissionB);

        var result = permissions.Has(PermissionA);

        result.Should().BeTrue();
    }

    [Fact]
    public void Has_WithMissingPermission_ShouldReturnFalse()
    {
        var permissions = Permissions.Create(PermissionA);

        var result = permissions.Has(PermissionB);

        result.Should().BeFalse();
    }

    [Fact]
    public void Has_WithNonePermission_ShouldReturnFalse()
    {
        var permissions = Permissions.Create(PermissionA);

        var result = permissions.Has(Permissions.None.Value);

        result.Should().BeFalse();
    }

    [Fact]
    public void Has_WithInvalidPermission_ShouldThrowDomainException()
    {
        var permissions = Permissions.Create(PermissionA);
        var invalidPermission = InvalidPermission;

        var act = () => permissions.Has(invalidPermission);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PermissionInvalid);
    }

    [Fact]
    public void ToString_ShouldReturnPermissionsAsString()
    {
        var permissions = Permissions.Create(PermissionA);

        permissions.ToString().Should().Be(PermissionA.ToString());
    }
}
