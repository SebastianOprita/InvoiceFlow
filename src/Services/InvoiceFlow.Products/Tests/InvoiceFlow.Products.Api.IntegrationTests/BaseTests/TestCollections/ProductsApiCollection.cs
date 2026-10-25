using Xunit;

namespace InvoiceFlow.Products.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class ProductsApiCollection : ICollectionFixture<ProductsApiFactory>
{
    public const string Name = "Products API Integration Tests";
}
