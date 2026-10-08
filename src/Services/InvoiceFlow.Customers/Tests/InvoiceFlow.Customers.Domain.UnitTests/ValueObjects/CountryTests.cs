using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class CountryTests
{
    [Fact]
    public void Create_ShouldReturnCountry_WhenValueIsValid()
    {
        var result = Country.Create("Romania");

        result.Value.Should().Be("Romania");
        result.ToString().Should().Be("Romania");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = Country.Create("  Romania  ");

        result.Value.Should().Be("Romania");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => Country.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CountryRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => Country.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CountryRequired);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', Country.MaxLength);

        var result = Country.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(Country.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', Country.MaxLength + 1);

        var act = () => Country.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CountryTooLong);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', Country.MaxLength)}  ";

        var result = Country.Create(value);

        result.Value.Should().Be(new string('A', Country.MaxLength));
    }
}
