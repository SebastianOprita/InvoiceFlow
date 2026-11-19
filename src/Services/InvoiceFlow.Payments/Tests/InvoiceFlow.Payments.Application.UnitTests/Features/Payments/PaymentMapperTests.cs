using FluentAssertions;
using InvoiceFlow.Payments.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class PaymentMapperTests
{
    [Fact]
    public void Maps_payment_fields_and_empty_allocations()
    {
        var payment = CreatePayment();
        var dto = payment.ToDto();
        dto.TenantId.Should().Be(payment.TenantId);
        dto.Id.Should().Be(payment.Id);
        dto.CustomerId.Should().Be(payment.CustomerId);
        dto.CurrencyCode.Should().Be("RON");
        dto.Amount.Should().Be(25m);
        dto.AllocatedAmount.Should().Be(0m);
        dto.UnallocatedAmount.Should().Be(25m);
        dto.Method.Should().Be(PaymentMethod.Card);
        dto.Reference.Should().Be("INVEST-1");
        dto.Notes.Should().Be("test note");
        dto.Allocations.Should().BeEmpty();
    }

    internal static Payment CreatePayment() => Payment.Create(Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        CurrencyCode.Create("RON"),
        Money.Create(25m, CurrencyCode.Create("RON")),
        PaymentMethod.Card,
        PaymentReference.Create("INVEST-1"),
        PaymentNotes.Create("test note"),
        DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
}
