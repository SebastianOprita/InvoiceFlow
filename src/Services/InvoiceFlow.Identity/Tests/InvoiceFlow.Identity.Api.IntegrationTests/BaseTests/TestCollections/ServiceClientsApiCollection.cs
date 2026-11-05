using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class ServiceClientsApiCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Service Clients API Integration Tests";
}
