using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class PhoneNumberTests
{
    [Fact]
    public void Create_ShouldReturnPhoneNumber_WhenValueIsValid()
    {
        var result = PhoneNumber.Create("+40123456789");

        result.Value.Should().Be("+40123456789");
        result.ToString().Should().Be("+40123456789");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = PhoneNumber.Create("  +40123456789  ");

        result.Value.Should().Be("+40123456789");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => PhoneNumber.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PhoneNumberRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => PhoneNumber.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PhoneNumberRequired);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('1', PhoneNumber.MaxLength);

        var result = PhoneNumber.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(PhoneNumber.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('1', PhoneNumber.MaxLength + 1);

        var act = () => PhoneNumber.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PhoneNumberTooLong);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('1', PhoneNumber.MaxLength)}  ";

        var result = PhoneNumber.Create(value);

        result.Value.Should().Be(new string('1', PhoneNumber.MaxLength));
    }

    [Fact]
    public void CreateOptional_ShouldReturnNull_WhenValueIsNull()
    {
        var result = PhoneNumber.CreateOptional(null);

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
        var result = PhoneNumber.CreateOptional(value);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_ShouldReturnPhoneNumber_WhenValueIsValid()
    {
        var result = PhoneNumber.CreateOptional("+40123456789");

        result.Should().NotBeNull();
        result!.Value.Should().Be("+40123456789");
    }

    [Fact]
    public void CreateOptional_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = PhoneNumber.CreateOptional("  +40123456789  ");

        result.Should().NotBeNull();
        result!.Value.Should().Be("+40123456789");
    }

    [Fact]
    public void CreateOptional_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('1', PhoneNumber.MaxLength + 1);

        var act = () => PhoneNumber.CreateOptional(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.PhoneNumberTooLong);
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = PhoneNumber.Create("  +40123456789  ");

        result.ToString().Should().Be("+40123456789");
    }
}
