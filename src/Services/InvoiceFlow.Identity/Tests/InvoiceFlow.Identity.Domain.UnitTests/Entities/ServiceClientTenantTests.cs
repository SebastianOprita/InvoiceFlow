using FluentAssertions;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public sealed class ServiceClientTenantTests
{
    [Fact]
    public void Create_StoresClientTenantAndCreationTimestamp()
    {
        var serviceClientId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var createdAtUtc = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var serviceClientTenant = ServiceClientTenant.Create(serviceClientId, tenantId, createdAtUtc);

        serviceClientTenant.ServiceClientId.Should().Be(serviceClientId);
        serviceClientTenant.TenantId.Should().Be(tenantId);
        serviceClientTenant.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Create_RejectsEmptyClientOrTenantId()
    {
        var tenantId = Guid.NewGuid();
        var serviceClientId = Guid.NewGuid();
        var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        FluentActions.Invoking(() => ServiceClientTenant.Create(Guid.Empty, tenantId, now))
            .Should().Throw<InvoiceFlow.BuildingBlocks.Domain.DomainException>()
            .WithDomainError(DomainErrors.ServiceClientIdRequired);
        FluentActions.Invoking(() => ServiceClientTenant.Create(serviceClientId, Guid.Empty, now))
            .Should().Throw<InvoiceFlow.BuildingBlocks.Domain.DomainException>()
            .WithDomainError(DomainErrors.TenantIdRequired);
    }

    [Fact]
    public void GrantTenant_CreatesAccessAndRevokeRemovesIt()
    {
        var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var organization = Organization.Create(Guid.NewGuid(), OrganizationName.Create("Org"), OrganizationTypes.Partner, now);
        var client = ServiceClient.Create(Guid.NewGuid(), organization, ServiceClientId.Create("service-client"), ServiceClientName.Create("Client"), now);
        var tenantId = Guid.NewGuid();

        client.GrantTenant(tenantId, now.AddMinutes(1));
        client.CanAccessTenant(tenantId).Should().BeTrue();
        client.Tenants.Single().CreatedAtUtc.Should().Be(now.AddMinutes(1));
        client.GrantTenant(tenantId, now.AddMinutes(2));
        client.Tenants.Should().HaveCount(1);
        client.RevokeTenant(tenantId, now.AddMinutes(3));
        client.CanAccessTenant(tenantId).Should().BeFalse();
    }
}
