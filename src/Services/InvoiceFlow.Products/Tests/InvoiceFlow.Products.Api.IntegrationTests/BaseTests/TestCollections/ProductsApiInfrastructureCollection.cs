using Xunit;

namespace InvoiceFlow.Products.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class ProductsApiInfrastructureCollection : ICollectionFixture<ProductsApiFactory>
{
    public const string Name = "Products API Infrastructure Integration Tests";
}
