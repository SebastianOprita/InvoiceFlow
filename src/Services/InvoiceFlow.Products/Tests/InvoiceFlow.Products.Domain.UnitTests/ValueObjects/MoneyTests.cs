using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Products.Domain.UnitTests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_ShouldReturnMoney_WhenAmountAndCurrencyAreValid()
    {
        var currencyCode = CurrencyCode.Create("USD");

        var result = Money.Create(12.34m, currencyCode);

        result.Amount.Should().Be(12.34m);
        result.CurrencyCode.Should().Be(currencyCode);
    }

    [Fact]
    public void Create_ShouldAllowZeroAmount()
    {
        var currencyCode = CurrencyCode.Create("USD");

        var result = Money.Create(0m, currencyCode);

        result.Amount.Should().Be(0m);
        result.CurrencyCode.Should().Be(currencyCode);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenCurrencyCodeIsNull()
    {
        var act = () => Money.Create(12.34m, null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CurrencyCodeRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenAmountIsNegative()
    {
        var currencyCode = CurrencyCode.Create("USD");

        var act = () => Money.Create(-0.01m, currencyCode);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.MoneyInvalid);
    }

    [Theory]
    [InlineData(1.234)]
    [InlineData(10.999)]
    public void Create_ShouldThrowDomainException_WhenAmountHasTooManyDecimalPlacesForCurrency(
        decimal amount)
    {
        var currencyCode = CurrencyCode.Create("USD");

        var act = () => Money.Create(amount, currencyCode);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.MoneyDecimalPlacesInvalid);
    }

    [Fact]
    public void Create_ShouldAllowAmountWithNoDecimalPlaces_ForCurrencyWithoutDecimalPlaces()
    {
        var currencyCode = CurrencyCode.Create("JPY");

        var result = Money.Create(100m, currencyCode);

        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenAmountHasDecimalPlacesForCurrencyWithoutDecimalPlaces()
    {
        var currencyCode = CurrencyCode.Create("JPY");

        var act = () => Money.Create(100.01m, currencyCode);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.MoneyDecimalPlacesInvalid);
    }

    [Fact]
    public void Create_ShouldAllowThreeDecimalPlaces_ForCurrencyWithThreeDecimalPlaces()
    {
        var currencyCode = CurrencyCode.Create("BHD");

        var result = Money.Create(1.234m, currencyCode);

        result.Amount.Should().Be(1.234m);
    }

    [Fact]
    public void Zero_ShouldReturnZeroMoney_ForCurrency()
    {
        var currencyCode = CurrencyCode.Create("USD");

        var result = Money.Zero(currencyCode);

        result.Amount.Should().Be(0m);
        result.CurrencyCode.Should().Be(currencyCode);
    }

    [Fact]
    public void Zero_ShouldThrowDomainException_WhenCurrencyCodeIsNull()
    {
        var act = () => Money.Zero(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CurrencyCodeRequired);
    }

    [Fact]
    public void ToString_ShouldFormatAmountUsingCurrencyDecimalPlaces()
    {
        var result = Money.Create(12.3m, CurrencyCode.Create("USD"));

        result.ToString().Should().Be("12.30 USD");
    }

    [Fact]
    public void ToString_ShouldOmitDecimalPlaces_WhenCurrencyHasNone()
    {
        var result = Money.Create(100m, CurrencyCode.Create("JPY"));

        result.ToString().Should().Be("100 JPY");
    }

    [Fact]
    public void ToString_ShouldFormatThreeDecimalPlaces_WhenCurrencyUsesThree()
    {
        var result = Money.Create(1.2m, CurrencyCode.Create("BHD"));

        result.ToString().Should().Be("1.200 BHD");
    }
}
