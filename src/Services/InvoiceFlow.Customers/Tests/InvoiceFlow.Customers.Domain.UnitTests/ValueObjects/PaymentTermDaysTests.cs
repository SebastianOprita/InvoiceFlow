using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class PaymentTermDaysTests
{
    [Fact]
    public void Create_ShouldReturnPaymentTermDays_WhenValueIsPositive()
    {
        var result = PaymentTermDays.Create(30);

        result.Value.Should().Be(30);
        result.ToString().Should().Be("30");
    }

    [Fact]
    public void Create_ShouldReturnPaymentTermDays_WhenValueIsZero()
    {
        var result = PaymentTermDays.Create(0);

        result.Value.Should().Be(0);
        result.ToString().Should().Be("0");
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNegative()
    {
        var act = () => PaymentTermDays.Create(-1);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.PaymentTermDaysInvalid.ErrorMessage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-30)]
    [InlineData(-365)]
    public void Create_ShouldThrowDomainException_ForAnyNegativeValue(int value)
    {
        var act = () => PaymentTermDays.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.PaymentTermDaysInvalid.ErrorMessage);
    }

    [Fact]
    public void ToString_ShouldReturnIntegerStringRepresentation()
    {
        var result = PaymentTermDays.Create(45);

        result.ToString().Should().Be("45");
    }
}
