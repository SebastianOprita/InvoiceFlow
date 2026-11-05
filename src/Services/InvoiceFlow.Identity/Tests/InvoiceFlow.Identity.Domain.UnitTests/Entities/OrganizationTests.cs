using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public sealed class OrganizationTests
{
    private static readonly DateTimeOffset CreatedAtUtc = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedAtUtc = CreatedAtUtc.AddHours(1);

    [Fact]
    public void Create_SetsPropertiesAndStartsActive()
    {
        var name = OrganizationName.Create("Acme");
        var organization = Organization.Create(Guid.NewGuid(), name, OrganizationTypes.Partner, CreatedAtUtc);

        organization.Name.Should().Be(name);
        organization.Type.Should().Be(OrganizationTypes.Partner);
        organization.IsActive.Should().BeTrue();
        organization.CreatedAtUtc.Should().Be(CreatedAtUtc);
        organization.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_RejectsEmptyIdNullNameAndUndefinedType()
    {
        FluentActions.Invoking(() => Organization.Create(Guid.Empty, OrganizationName.Create("Acme"), OrganizationTypes.Partner, CreatedAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.OrganizationIdRequired);
        FluentActions.Invoking(() => Organization.Create(Guid.NewGuid(), null!, OrganizationTypes.Partner, CreatedAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.OrganizationNameRequired);
        FluentActions.Invoking(() => Organization.Create(Guid.NewGuid(), OrganizationName.Create("Acme"), (OrganizationTypes)999, CreatedAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.OrganizationTypeInvalid);
    }

    [Fact]
    public void Rename_ChangesNameAndTimestamp()
    {
        var organization = CreateOrganization();
        var name = OrganizationName.Create("New name");

        organization.Rename(name, UpdatedAtUtc);

        organization.Name.Should().Be(name);
        organization.UpdatedAtUtc.Should().Be(UpdatedAtUtc);
    }

    [Fact]
    public void Rename_DoesNothingWhenNameIsUnchanged()
    {
        var organization = CreateOrganization();
        organization.Rename(organization.Name, UpdatedAtUtc);
        organization.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Rename_RejectsNullNameAndInvalidTimestamps()
    {
        var organization = CreateOrganization();
        FluentActions.Invoking(() => organization.Rename(null!, UpdatedAtUtc))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.OrganizationNameRequired);
        var defaultTimestampOrganization = CreateOrganization();
        FluentActions.Invoking(() => defaultTimestampOrganization.Rename(OrganizationName.Create("Changed"), default))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedAtUtcRequired);
        var earlierTimestampOrganization = CreateOrganization();
        FluentActions.Invoking(() => earlierTimestampOrganization.Rename(OrganizationName.Create("Changed"), CreatedAtUtc.AddTicks(-1)))
            .Should().Throw<DomainException>().WithDomainError(DomainErrors.UpdatedAtUtcInvalid);
    }

    [Fact]
    public void ActivateAndDeactivate_AreIdempotentAndUpdateWhenStateChanges()
    {
        var organization = CreateOrganization();

        organization.Activate(UpdatedAtUtc);
        organization.IsActive.Should().BeTrue();
        organization.UpdatedAtUtc.Should().BeNull();

        organization.Deactivate(UpdatedAtUtc);
        organization.IsActive.Should().BeFalse();
        organization.UpdatedAtUtc.Should().Be(UpdatedAtUtc);

        organization.Deactivate(UpdatedAtUtc.AddHours(1));
        organization.UpdatedAtUtc.Should().Be(UpdatedAtUtc);

        organization.Activate(UpdatedAtUtc.AddHours(2));
        organization.IsActive.Should().BeTrue();
        organization.UpdatedAtUtc.Should().Be(UpdatedAtUtc.AddHours(2));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StateChanges_RejectInvalidTimestamp(bool activate)
    {
        var organization = CreateOrganization();
        if (activate)
            organization.Deactivate(UpdatedAtUtc);

        FluentActions.Invoking(() =>
                {
                    if (activate)
                        organization.Activate(default);
                    else
                        organization.Deactivate(CreatedAtUtc.AddTicks(-1));
                })
            .Should().Throw<DomainException>();
    }

    private static Organization CreateOrganization() =>
        Organization.Create(Guid.NewGuid(), OrganizationName.Create("Acme"), OrganizationTypes.Partner, CreatedAtUtc);
}
