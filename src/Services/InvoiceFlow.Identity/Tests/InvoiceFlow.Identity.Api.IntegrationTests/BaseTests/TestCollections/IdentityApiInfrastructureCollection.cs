using Xunit;

namespace InvoiceFlow.Identity.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class IdentityApiInfrastructureCollection : ICollectionFixture<IdentityApiFactory>
{
    public const string Name = "Identity API Infrastructure Integration Tests";
}
