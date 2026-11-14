using Xunit;

namespace InvoiceFlow.Payments.Api.IntegrationTests.BaseTests;

[CollectionDefinition(Name)]
public sealed class PaymentsApiInfrastructureCollection : ICollectionFixture<PaymentsApiFactory>
{
    public const string Name = "Payments API Infrastructure Integration Tests";
}
