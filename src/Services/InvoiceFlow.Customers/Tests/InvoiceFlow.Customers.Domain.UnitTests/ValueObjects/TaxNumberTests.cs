using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class TaxNumberTests
{
    [Fact]
    public void Create_ShouldReturnTaxNumber_WhenValueIsValid()
    {
        var result = TaxNumber.Create("RO12345678");

        result.Value.Should().Be("RO12345678");
        result.ToString().Should().Be("RO12345678");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = TaxNumber.Create("  RO12345678  ");

        result.Value.Should().Be("RO12345678");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => TaxNumber.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.TaxNumberRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => TaxNumber.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.TaxNumberRequired);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', TaxNumber.MaxLength);

        var result = TaxNumber.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(TaxNumber.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', TaxNumber.MaxLength + 1);

        var act = () => TaxNumber.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.TaxNumberTooLong);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', TaxNumber.MaxLength)}  ";

        var result = TaxNumber.Create(value);

        result.Value.Should().Be(new string('A', TaxNumber.MaxLength));
    }

    [Fact]
    public void CreateOptional_ShouldReturnNull_WhenValueIsNull()
    {
        var result = TaxNumber.CreateOptional(null);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void CreateOptional_ShouldReturnNull_WhenValueIsWhiteSpace(string value)
    {
        var result = TaxNumber.CreateOptional(value);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_ShouldReturnTaxNumber_WhenValueIsValid()
    {
        var result = TaxNumber.CreateOptional("RO12345678");

        result.Should().NotBeNull();
        result!.Value.Should().Be("RO12345678");
    }

    [Fact]
    public void CreateOptional_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = TaxNumber.CreateOptional("  RO12345678  ");

        result.Should().NotBeNull();
        result!.Value.Should().Be("RO12345678");
    }

    [Fact]
    public void CreateOptional_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', TaxNumber.MaxLength + 1);

        var act = () => TaxNumber.CreateOptional(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.TaxNumberTooLong);
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = TaxNumber.Create("  RO12345678  ");

        result.ToString().Should().Be("RO12345678");
    }
}
