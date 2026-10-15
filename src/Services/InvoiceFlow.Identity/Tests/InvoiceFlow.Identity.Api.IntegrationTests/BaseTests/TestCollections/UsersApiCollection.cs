using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public class UsersApiCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Users API Integration Tests";
}
