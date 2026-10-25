using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public class RoleTests
{
    private static readonly Guid TenantId = Guid.CreateVersion7();
    private static readonly Guid RoleId = Guid.CreateVersion7();
    private static readonly DateTimeOffset CreatedAtUtc = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedAtUtc = CreatedAtUtc.AddHours(1);
    private static readonly long Permission = 1L << 3;

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
        role.IsActive.Should().BeTrue();
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
    public void Deactivate_Should_MarkRoleInactiveAndSetUpdatedAtUtc()
    {
        var role = CreateRole();

        role.Deactivate(UpdatedAtUtc);

        role.IsActive.Should().BeFalse();
        role.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void Activate_Should_MarkRoleActiveAndSetUpdatedAtUtc()
    {
        var role = CreateRole();
        role.Deactivate(UpdatedAtUtc);
        var activatedAtUtc = UpdatedAtUtc.AddHours(1);

        role.Activate(activatedAtUtc);

        role.IsActive.Should().BeTrue();
        role.UpdatedAtUtc.Should().Be(activatedAtUtc);
    }

    [Fact]
    public void Deactivate_Should_NotUpdateTimestamp_WhenRoleIsAlreadyInactive()
    {
        var role = CreateRole();
        role.Deactivate(UpdatedAtUtc);

        role.Deactivate(UpdatedAtUtc.AddHours(1));

        role.IsActive.Should().BeFalse();
        role.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void Activate_Should_NotUpdateTimestamp_WhenRoleIsAlreadyActive()
    {
        var role = CreateRole();

        role.Activate(UpdatedAtUtc);

        role.IsActive.Should().BeTrue();
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
        role => { role.GrantPermission(Permission, UpdatedAtUtc); role.RevokePermission(Permission, default); },
        role => role.Deactivate(default),
        role => { role.Deactivate(UpdatedAtUtc); role.Activate(default); },
    };

    public static TheoryData<Action<Role>> UpdateActionsBeforeCreatedAt => new()
    {
        role => role.UpdateDetails(RoleName.Create("New name"), CreatedAtUtc.AddTicks(-1)),
        role => role.GrantPermission(Permission, CreatedAtUtc.AddTicks(-1)),
        role => { role.GrantPermission(Permission, UpdatedAtUtc); role.RevokePermission(Permission, CreatedAtUtc.AddTicks(-1)); },
        role => role.Deactivate(CreatedAtUtc.AddTicks(-1)),
        role => { role.Deactivate(UpdatedAtUtc); role.Activate(CreatedAtUtc.AddTicks(-1)); },
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
