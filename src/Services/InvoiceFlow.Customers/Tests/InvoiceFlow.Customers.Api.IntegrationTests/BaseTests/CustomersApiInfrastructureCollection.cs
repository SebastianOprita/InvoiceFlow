using Xunit;

namespace InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class CustomersApiInfrastructureCollection : ICollectionFixture<CustomersApiFactory>
{
    public const string Name = "Customers API Infrastructure Integration Tests";
}
