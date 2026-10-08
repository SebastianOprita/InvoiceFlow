using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public class RoleTests
{
    private static readonly Guid TenantId = Guid.CreateVersion7();
    private static readonly Guid RoleId = Guid.CreateVersion7();
    private static readonly DateTime CreatedAtUtc = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UpdatedAtUtc = CreatedAtUtc.AddHours(1);
    private static readonly SystemPermission Permission = SystemPermission.CustomerView;

    [Fact]
    public void Constructor_Should_CreateRole_WithRequiredValues()
    {
        var name = RoleName.Create("Admin");

        var role = Role.Create(TenantId, RoleId, name, CreatedAtUtc);

        role.TenantId.Should().Be(TenantId);
        role.Id.Should().Be(RoleId);
        role.Name.Should().Be(name);
        role.Description.Should().BeNull();
        role.Permissions.Should().Be(RolePermissions.None);
        role.CreatedAtUtc.Should().Be(CreatedAtUtc);
        role.UpdatedAtUtc.Should().BeNull();
        role.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Should_CreateRole_WithDescriptionAndPermissions()
    {
        var name = RoleName.Create("Manager");
        var description = RoleDescription.Create("Can manage invoices");
        var permissions = RolePermissions.None.Grant(Permission);

        var role = Role.Create(TenantId, RoleId, name, CreatedAtUtc, description, permissions);

        role.Description.Should().Be(description);
        role.Permissions.Should().Be(permissions);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenTenantIdIsEmpty()
    {
        var act = () => Role.Create(Guid.Empty, RoleId, RoleName.Create("Admin"), CreatedAtUtc);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenIdIsEmpty()
    {
        var act = () => Role.Create(TenantId, Guid.Empty, RoleName.Create("Admin"), CreatedAtUtc);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenCreatedAtUtcIsDefault()
    {
        var act = () => Role.Create(TenantId, RoleId, RoleName.Create("Admin"), default);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CreatedAtUtcRequired);
    }

    [Fact]
    public void UpdateDetails_Should_UpdateNameDescriptionAndUpdatedAtUtc()
    {
        var role = CreateRole();
        var newName = RoleName.Create("Accountant");
        var newDescription = RoleDescription.Create("Handles invoices");

        role.UpdateDetails(newName, UpdatedAtUtc, newDescription);

        role.Name.Should().Be(newName);
        role.Description.Should().Be(newDescription);
        role.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_Should_NotUpdateUpdatedAtUtc_WhenDetailsAreUnchanged()
    {
        var name = RoleName.Create("Admin");
        var description = RoleDescription.Create("Administrator role");
        var role = CreateRole(name, description);

        role.UpdateDetails(name, UpdatedAtUtc, description);

        role.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void GrantPermission_Should_AddPermissionAndSetUpdatedAtUtc()
    {
        var role = CreateRole();

        role.GrantPermission(Permission, UpdatedAtUtc);

        role.HasPermission(Permission).Should().BeTrue();
        role.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void GrantPermission_Should_NotUpdateUpdatedAtUtc_WhenPermissionAlreadyGranted()
    {
        var role = CreateRole();
        role.GrantPermission(Permission, UpdatedAtUtc);

        role.GrantPermission(Permission, UpdatedAtUtc.AddHours(1));

        role.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void RevokePermission_Should_RemovePermissionAndSetUpdatedAtUtc()
    {
        var role = CreateRole();
        role.GrantPermission(Permission, UpdatedAtUtc);

        var revokedAtUtc = UpdatedAtUtc.AddHours(1);
        role.RevokePermission(Permission, revokedAtUtc);

        role.HasPermission(Permission).Should().BeFalse();
        role.UpdatedAtUtc.Should().Be(revokedAtUtc);
    }

    [Fact]
    public void RevokePermission_Should_NotUpdateUpdatedAtUtc_WhenPermissionIsNotGranted()
    {
        var role = CreateRole();

        role.RevokePermission(Permission, UpdatedAtUtc);

        role.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void SetPermissions_Should_UpdatePermissionsAndUpdatedAtUtc()
    {
        var role = CreateRole();
        var permissions = RolePermissions.None.Grant(Permission);

        role.SetPermissions(permissions, UpdatedAtUtc);

        role.Permissions.Should().Be(permissions);
        role.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void SetPermissions_Should_NotUpdateUpdatedAtUtc_WhenPermissionsAreUnchanged()
    {
        var role = CreateRole();

        role.SetPermissions(RolePermissions.None, UpdatedAtUtc);

        role.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(UpdateActions))]
    public void UpdateMethods_Should_Throw_WhenUpdatedAtUtcIsDefault(Action<Role> updateAction)
    {
        var role = CreateRole();

        var act = () => updateAction(role);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.UpdatedAtUtcRequired);
    }

    [Theory]
    [MemberData(nameof(UpdateActionsBeforeCreatedAt))]
    public void UpdateMethods_Should_Throw_WhenUpdatedAtUtcIsEarlierThanCreatedAtUtc(Action<Role> updateAction)
    {
        var role = CreateRole();

        var act = () => updateAction(role);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.UpdatedAtUtcInvalid);
    }

    public static TheoryData<Action<Role>> UpdateActions => new()
    {
        role => role.UpdateDetails(RoleName.Create("New name"), default),
        role => role.GrantPermission(Permission, default),
        role => role.RevokePermission(Permission, default),
        role => role.SetPermissions(RolePermissions.None.Grant(Permission), default)
    };

    public static TheoryData<Action<Role>> UpdateActionsBeforeCreatedAt => new()
    {
        role => role.UpdateDetails(RoleName.Create("New name"), CreatedAtUtc.AddTicks(-1)),
        role => role.GrantPermission(Permission, CreatedAtUtc.AddTicks(-1)),
        role => role.RevokePermission(Permission, CreatedAtUtc.AddTicks(-1)),
        role => role.SetPermissions(RolePermissions.None.Grant(Permission), CreatedAtUtc.AddTicks(-1))
    };

    private static Role CreateRole(
        RoleName? name = null,
        RoleDescription? description = null,
        RolePermissions? permissions = null)
    {
        return Role.Create(
            TenantId,
            RoleId,
            name ?? RoleName.Create("Admin"),
            CreatedAtUtc,
            description,
            permissions);
    }
}
