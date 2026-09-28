using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class PostalCodeTests
{
    [Fact]
    public void Create_ShouldReturnPostalCode_WhenValueIsValid()
    {
        var result = PostalCode.Create("123456");

        result.Value.Should().Be("123456");
        result.ToString().Should().Be("123456");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = PostalCode.Create("  123456  ");

        result.Value.Should().Be("123456");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => PostalCode.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.PostalCodeRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => PostalCode.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.PostalCodeRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('1', PostalCode.MaxLength);

        var result = PostalCode.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(PostalCode.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('1', PostalCode.MaxLength + 1);

        var act = () => PostalCode.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.PostalCodeTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('1', PostalCode.MaxLength)}  ";

        var result = PostalCode.Create(value);

        result.Value.Should().Be(new string('1', PostalCode.MaxLength));
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = PostalCode.Create("  123456  ");

        result.ToString().Should().Be("123456");
    }
}
