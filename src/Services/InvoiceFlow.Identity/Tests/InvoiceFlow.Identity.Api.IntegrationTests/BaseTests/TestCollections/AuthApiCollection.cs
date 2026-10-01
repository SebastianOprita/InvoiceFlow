using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;


[CollectionDefinition(Name)]
public sealed class AuthApiCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Auth API Integration Tests";
}
