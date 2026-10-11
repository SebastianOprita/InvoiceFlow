using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;


[CollectionDefinition(Name)]
public sealed class PlatformAuthApiCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Platform Auth API Integration Tests";
}
