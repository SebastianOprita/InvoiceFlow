using Xunit;

namespace InvoiceFlow.Customers.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class CustomersApiCollection : ICollectionFixture<CustomersApiFactory>
{
    public const string Name = "Customers API Integration Tests";
}
