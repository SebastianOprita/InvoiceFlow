using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.Entities;

public sealed class ServiceClientSecretTests
{
    [Fact]
    public void AddSecret_StoresValuesAndTracksActiveState()
    {
        var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var organization = Organization.Create(Guid.NewGuid(), OrganizationName.Create("Org"), OrganizationTypes.Partner, now);
        var client = ServiceClient.Create(Guid.NewGuid(), organization, ServiceClientId.Create("service-client"), ServiceClientName.Create("Client"), now);
        var secret = client.AddSecret(Guid.NewGuid(), ServiceClientSecretHash.Create(new string('A', 64)), ServiceClientSecretName.Create("deploy"), null, now.AddDays(1), now);

        secret.ServiceClientId.Should().Be(client.Id);
        secret.IsActive(now).Should().BeTrue();
        secret.Description.Should().BeNull();
        secret.IsActive(now.AddDays(1)).Should().BeFalse();
    }

    [Fact]
    public void AddSecret_RejectsExpirationAtOrBeforeCreation()
    {
        var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var organization = Organization.Create(Guid.NewGuid(), OrganizationName.Create("Org"), OrganizationTypes.Partner, now);
        var client = ServiceClient.Create(Guid.NewGuid(), organization, ServiceClientId.Create("service-client"), ServiceClientName.Create("Client"), now);
        FluentActions.Invoking(() => client.AddSecret(Guid.NewGuid(), ServiceClientSecretHash.Create(new string('A', 64)), ServiceClientSecretName.Create("deploy"), null, now, now)).Should().Throw<DomainException>();
    }
}
