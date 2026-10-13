using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class RolesApiCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Roles API Integration Tests";
}
