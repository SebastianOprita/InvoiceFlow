using Xunit;

namespace InvoiceFlow.Invoices.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class InvoicesApiInfrastructureCollection : ICollectionFixture<InvoicesApiFactory>
{
    public const string Name = "Invoices API Infrastructure Integration Tests";
}
