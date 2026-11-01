using FluentAssertions;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Application.UnitTests.Features.ClientSecrets;

public sealed class ServiceClientSecretMapperTests
{
    [Fact]
    public void ToDto_MapsSecretMetadataWithoutExposingHash()
    {
        var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var organization = Organization.Create(Guid.NewGuid(), OrganizationName.Create("Org"), OrganizationTypes.Partner, now);
        var client = ServiceClient.Create(Guid.NewGuid(), organization, ServiceClientId.Create("service-client"), ServiceClientName.Create("Client"), now);
        var secret = client.AddSecret(Guid.NewGuid(), ServiceClientSecretHash.Create(new string('D', 64)), ServiceClientSecretName.Create("deploy"), ServiceClientSecretDescription.Create("CI"), now.AddDays(5), now);
        client.RevokeSecret(secret.Id, now.AddDays(1));

        var dto = secret.ToDto();

        dto.Id.Should().Be(secret.Id);
        dto.ServiceClientId.Should().Be(client.Id);
        dto.Name.Should().Be("deploy");
        dto.Description.Should().Be("CI");
        dto.CreatedAtUtc.Should().Be(now);
        dto.ExpiresAtUtc.Should().Be(now.AddDays(5));
        dto.RevokedAtUtc.Should().Be(now.AddDays(1));
    }
}
