using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class CityTests
{
    [Fact]
    public void Create_ShouldReturnCity_WhenValueIsValid()
    {
        var result = City.Create("Bucharest");

        result.Value.Should().Be("Bucharest");
        result.ToString().Should().Be("Bucharest");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = City.Create("  Bucharest  ");

        result.Value.Should().Be("Bucharest");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => City.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.CityRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => City.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.CityRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', City.MaxLength);

        var result = City.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(City.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', City.MaxLength + 1);

        var act = () => City.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.CityTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', City.MaxLength)}  ";

        var result = City.Create(value);

        result.Value.Should().Be(new string('A', City.MaxLength));
    }
}
