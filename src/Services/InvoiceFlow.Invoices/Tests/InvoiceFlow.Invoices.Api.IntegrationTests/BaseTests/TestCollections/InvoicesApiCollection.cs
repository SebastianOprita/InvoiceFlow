using Xunit;

namespace InvoiceFlow.Invoices.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class InvoicesApiCollection : ICollectionFixture<InvoicesApiFactory>
{
    public const string Name = "Invoices API Integration Tests";
}
