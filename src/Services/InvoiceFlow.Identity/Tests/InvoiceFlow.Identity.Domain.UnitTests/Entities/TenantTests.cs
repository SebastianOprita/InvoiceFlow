using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public class TenantTests
{
    private static readonly Guid TenantId = Guid.CreateVersion7();
    private static readonly DateTime CreatedAtUtc = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime UpdatedAtUtc = CreatedAtUtc.AddHours(1);

    [Fact]
    public void Constructor_Should_CreateTenant_WithValidValues()
    {
        var name = TenantName.Create("Acme");
        var slug = TenantSlug.Create("acme");

        var tenant = Tenant.Create(TenantId, name, slug, CreatedAtUtc);

        tenant.Id.Should().Be(TenantId);
        tenant.Name.Should().Be(name);
        tenant.Slug.Should().Be(slug);
        tenant.IsActive.Should().BeTrue();
        tenant.CreatedAtUtc.Should().Be(CreatedAtUtc);
        tenant.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Constructor_Should_Throw_WhenIdIsEmpty()
    {
        var act = () => Tenant.Create(
            Guid.Empty,
            TenantName.Create("Acme"),
            TenantSlug.Create("acme"),
            CreatedAtUtc);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.IdRequired);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenCreatedAtUtcIsDefault()
    {
        var act = () => Tenant.Create(
            TenantId,
            TenantName.Create("Acme"),
            TenantSlug.Create("acme"),
            default);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CreatedAtUtcRequired);
    }

    [Fact]
    public void UpdateName_Should_UpdateNameAndUpdatedAtUtc()
    {
        var tenant = CreateTenant();
        var newName = TenantName.Create("Updated Tenant");

        tenant.UpdateName(newName, UpdatedAtUtc);

        tenant.Name.Should().Be(newName);
        tenant.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void UpdateName_Should_NotUpdateUpdatedAtUtc_WhenNameIsUnchanged()
    {
        var name = TenantName.Create("Acme");
        var tenant = CreateTenant(name);

        tenant.UpdateName(name, UpdatedAtUtc);

        tenant.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Deactivate_Should_SetIsActiveToFalse_AndUpdateUpdatedAtUtc()
    {
        var tenant = CreateTenant();

        tenant.Deactivate(UpdatedAtUtc);

        tenant.IsActive.Should().BeFalse();
        tenant.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void Deactivate_Should_NotUpdateUpdatedAtUtc_WhenAlreadyInactive()
    {
        var tenant = CreateTenant();

        tenant.Deactivate(UpdatedAtUtc);

        tenant.Deactivate(UpdatedAtUtc.AddHours(1));

        tenant.IsActive.Should().BeFalse();
        tenant.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void Activate_Should_SetIsActiveToTrue_AndUpdateUpdatedAtUtc()
    {
        var tenant = CreateTenant();
        tenant.Deactivate(UpdatedAtUtc);

        var activatedAtUtc = UpdatedAtUtc.AddHours(1);

        tenant.Activate(activatedAtUtc);

        tenant.IsActive.Should().BeTrue();
        tenant.UpdatedAtUtc.Should().Be(activatedAtUtc);
    }

    [Fact]
    public void Activate_Should_NotUpdateUpdatedAtUtc_WhenAlreadyActive()
    {
        var tenant = CreateTenant();

        tenant.Activate(UpdatedAtUtc);

        tenant.IsActive.Should().BeTrue();
        tenant.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(UpdateActionsWithDefaultDate))]
    public void UpdateMethods_Should_Throw_WhenUpdatedAtUtcIsDefault(Action<Tenant> updateAction)
    {
        var tenant = CreateTenant();

        var act = () => updateAction(tenant);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.UpdatedAtUtcRequired);
    }

    [Theory]
    [MemberData(nameof(UpdateActionsWithInvalidDate))]
    public void UpdateMethods_Should_Throw_WhenUpdatedAtUtcIsEarlierThanCreatedAtUtc(Action<Tenant> updateAction)
    {
        var tenant = CreateTenant();

        var act = () => updateAction(tenant);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.UpdatedAtUtcInvalid);
    }

    public static TheoryData<Action<Tenant>> UpdateActionsWithDefaultDate => new()
    {
        tenant => tenant.UpdateName(TenantName.Create("Updated"), default),
        tenant => tenant.Deactivate(default),
        tenant => tenant.Activate(default)
    };

    public static TheoryData<Action<Tenant>> UpdateActionsWithInvalidDate => new()
    {
        tenant => tenant.UpdateName(TenantName.Create("Updated"), CreatedAtUtc.AddTicks(-1)),
        tenant => tenant.Deactivate(CreatedAtUtc.AddTicks(-1)),
        tenant => tenant.Activate(CreatedAtUtc.AddTicks(-1))
    };

    private static Tenant CreateTenant(TenantName? name = null)
    {
        return Tenant.Create(
            TenantId,
            name ?? TenantName.Create("Acme"),
            TenantSlug.Create("acme"),
            CreatedAtUtc);
    }
}

