using FluentAssertions;
using InvoiceFlow.Invoices.Domain;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class InvoiceMapperTests
{
    [Fact]
    public void ToDto_ShouldMapInvoiceAndCustomerSnapshot()
    {
        var invoice = InvoiceTestData.DraftInvoice();

        var dto = invoice.ToDto();

        dto.Id.Should().Be(invoice.Id);
        dto.TenantId.Should().Be(invoice.TenantId);
        dto.Number.Should().BeNull();
        dto.CustomerId.Should().Be(invoice.CustomerId);
        dto.CustomerCode.Should().Be("CUST-001");
        dto.CustomerName.Should().Be("Acme Ltd");
        dto.CustomerEmail.Should().Be("contact@acme.test");
        dto.CustomerPhone.Should().Be("+40700111222");
        dto.CustomerTaxNumber.Should().Be("RO123456");
        dto.CustomerRegistrationNumber.Should().Be("J40/123/2024");
        dto.CustomerAddressLine1.Should().Be("Main Street 1");
        dto.CustomerAddressLine2.Should().Be("Building A");
        dto.CustomerCity.Should().Be("Bucharest");
        dto.CustomerState.Should().Be("Bucharest");
        dto.CustomerCountry.Should().Be("Romania");
        dto.CustomerPostalCode.Should().Be("010101");
        dto.CurrencyCode.Should().Be("RON");
        dto.PaymentTermDays.Should().Be(30);
        dto.Status.Should().Be(InvoiceStatus.Draft);
        dto.PaymentStatus.Should().Be(InvoicePaymentStatus.Unpaid);
        dto.DiscountAmount.Should().Be(0);
        dto.NetAmount.Should().Be(100);
        dto.TaxAmount.Should().Be(19);
        dto.GrossAmount.Should().Be(119);
        dto.PaidAmount.Should().Be(0);
        dto.RemainingAmount.Should().Be(119);
        dto.Notes.Should().Be("Invoice notes");
        dto.CreatedAtUtc.Should().Be(invoice.CreatedAtUtc);
        dto.Lines.Should().ContainSingle().Which.Should().BeEquivalentTo(new InvoiceLineDto(
            invoice.Lines.Single().Id, 1, invoice.Lines.Single().ProductId, "PROD-001", "Professional service",
            1, UnitOfMeasure.Service, 100, 0, 0, 19, 19, 100, 119));
    }

    [Fact]
    public void ToDto_ShouldMapNullableCustomerFieldsAsNull()
    {
        var customer = InvoiceCustomerSnapshot.Create(
            InvoiceTestData.TenantId, Guid.CreateVersion7(), true,
            CustomerCode.Create("CUST-002"), CustomerName.Create("No Contact Ltd"),
            null, null, null, RegistrationNumber.Create("REG-2"), AddressLine1.Create("Road 2"),
            null, City.Create("Iasi"), null, Country.Create("Romania"), PostalCode.Create("700000"),
            CurrencyCode.Create("RON"), CreditLimit.Create(1000m), PaymentTermDays.Create(0));
        var invoice = Invoice.Create(InvoiceTestData.TenantId, Guid.CreateVersion7(), customer, null,
            new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero));

        var dto = invoice.ToDto();

        dto.CustomerEmail.Should().BeNull();
        dto.CustomerPhone.Should().BeNull();
        dto.CustomerTaxNumber.Should().BeNull();
        dto.CustomerAddressLine2.Should().BeNull();
        dto.CustomerState.Should().BeNull();
        dto.Notes.Should().BeNull();
    }
}
