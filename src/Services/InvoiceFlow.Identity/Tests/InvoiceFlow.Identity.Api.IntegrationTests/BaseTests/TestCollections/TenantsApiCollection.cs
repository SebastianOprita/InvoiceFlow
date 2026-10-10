using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class TenantsApiCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Tenants API Integration Tests";
}
