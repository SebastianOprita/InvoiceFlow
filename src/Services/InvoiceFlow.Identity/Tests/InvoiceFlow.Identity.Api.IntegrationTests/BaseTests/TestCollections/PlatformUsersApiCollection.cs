using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;


[CollectionDefinition(Name)]
public sealed class PlatformUsersApiCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Platform Users API Integration Tests";
}
