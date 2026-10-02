using FluentAssertions;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Currency.Tests;

public sealed class Iso4217Tests
{
    [Theory]
    [InlineData("EUR", 978, 2)]
    [InlineData("USD", 840, 2)]
    [InlineData("JPY", 392, 0)]
    [InlineData("KWD", 414, 3)]
    public void Get_WithKnownCurrency_ShouldReturnExpectedDefinition(
        string code,
        short numericCode,
        byte decimalPlaces)
    {
        // Act
        var currency = Iso4217.Get(code);

        // Assert
        currency.Code.Should().Be(code);
        currency.NumericCode.Should().Be(numericCode);
        currency.DecimalPlaces.Should().Be(decimalPlaces);
    }

    [Theory]
    [InlineData("eur")]
    [InlineData("Eur")]
    [InlineData("eUr")]
    public void Get_ShouldBeCaseInsensitive(string code)
    {
        // Act
        var currency = Iso4217.Get(code);

        // Assert
        currency.Code.Should().Be("EUR");
    }

    [Fact]
    public void Get_WithUnknownCurrency_ShouldThrowKeyNotFoundException()
    {
        // Act
        var action = () => Iso4217.Get("XXX");

        // Assert
        action.Should()
            .Throw<KeyNotFoundException>()
            .WithMessage("Unknown currency 'XXX'.");
    }

    [Fact]
    public void TryGet_WithKnownCurrency_ShouldReturnTrueAndCurrency()
    {
        // Act
        var result = Iso4217.TryGet("RON", out var currency);

        // Assert
        result.Should().BeTrue();
        currency.Should().NotBeNull();
        currency!.Code.Should().Be("RON");
        currency.NumericCode.Should().Be(946);
        currency.DecimalPlaces.Should().Be(2);
    }

    [Fact]
    public void TryGet_WithUnknownCurrency_ShouldReturnFalseAndNull()
    {
        // Act
        var result = Iso4217.TryGet("XXX", out var currency);

        // Assert
        result.Should().BeFalse();
        currency.Should().BeNull();
    }

    [Fact]
    public void All_ShouldContainConfiguredCurrencies()
    {
        // Assert
        Iso4217.All.Should().Contain(currency => currency.Code == "EUR");
        Iso4217.All.Should().Contain(currency => currency.Code == "JPY");
        Iso4217.All.Should().Contain(currency => currency.Code == "KWD");
    }

    [Fact]
    public void All_ShouldHaveUniqueCurrencyCodes()
    {
        var codes = Iso4217.All.Select(x => x.Code);

        codes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void All_ShouldHaveUniqueNumericCodes()
    {
        var numericCodes = Iso4217.All.Select(x => x.NumericCode);

        numericCodes.Should().OnlyHaveUniqueItems();
    }
}
