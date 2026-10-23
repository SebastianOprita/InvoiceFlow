using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Products.Domain.UnitTests.ValueObjects;

public sealed class CurrencyCodeTests
{
    [Fact]
    public void Create_ShouldReturnCurrencyCode_WhenValueIsValid()
    {
        var result = CurrencyCode.Create("USD");

        result.Value.Should().Be("USD");
        result.ToString().Should().Be("USD");
    }

    [Fact]
    public void Create_ShouldConvertValueToUpperCase()
    {
        var result = CurrencyCode.Create("usd");

        result.Value.Should().Be("USD");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = CurrencyCode.Create("  usd  ");

        result.Value.Should().Be("USD");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => CurrencyCode.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CurrencyCodeRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => CurrencyCode.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CurrencyCodeRequired);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("ZZZ")]
    [InlineData("1US")]
    [InlineData("U$D")]
    public void Create_ShouldThrowDomainException_WhenCurrencyCodeIsInvalid(string value)
    {
        var act = () => CurrencyCode.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CurrencyCodeInvalid);
    }

    [Fact]
    public void DecimalPlaces_ShouldReturnCurrencyDecimalPlaces()
    {
        var result = CurrencyCode.Create("USD");

        result.DecimalPlaces.Should().Be(2);
    }

    [Fact]
    public void DecimalPlaces_ShouldReturnZero_ForCurrencyWithoutDecimalPlaces()
    {
        var result = CurrencyCode.Create("JPY");

        result.DecimalPlaces.Should().Be(0);
    }

    [Fact]
    public void NumericCode_ShouldReturnIsoNumericCode()
    {
        var result = CurrencyCode.Create("USD");

        result.NumericCode.Should().Be(840);
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = CurrencyCode.Create("  usd  ");

        result.ToString().Should().Be("USD");
    }
}
