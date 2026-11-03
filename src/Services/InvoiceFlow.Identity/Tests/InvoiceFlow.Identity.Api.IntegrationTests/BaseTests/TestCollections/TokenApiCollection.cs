using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class TokenApiCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Token API Integration Tests";
}
