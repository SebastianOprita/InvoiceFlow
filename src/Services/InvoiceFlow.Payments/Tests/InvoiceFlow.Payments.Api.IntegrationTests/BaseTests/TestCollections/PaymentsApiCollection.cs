using Xunit;

namespace InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class PaymentsApiCollection : ICollectionFixture<PaymentsApiFactory>
{
    public const string Name = "Payments API Integration Tests";
}
