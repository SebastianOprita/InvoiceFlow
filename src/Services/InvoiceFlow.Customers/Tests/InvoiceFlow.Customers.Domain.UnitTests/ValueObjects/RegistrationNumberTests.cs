using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class RegistrationNumberTests
{
    [Fact]
    public void Create_ShouldReturnRegistrationNumber_WhenValueIsValid()
    {
        var result = RegistrationNumber.Create("J40/1234/2024");

        result.Value.Should().Be("J40/1234/2024");
        result.ToString().Should().Be("J40/1234/2024");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = RegistrationNumber.Create("  J40/1234/2024  ");

        result.Value.Should().Be("J40/1234/2024");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => RegistrationNumber.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.RegistrationNumberRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => RegistrationNumber.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.RegistrationNumberRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', RegistrationNumber.MaxLength);

        var result = RegistrationNumber.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(RegistrationNumber.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', RegistrationNumber.MaxLength + 1);

        var act = () => RegistrationNumber.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.RegistrationNumberTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', RegistrationNumber.MaxLength)}  ";

        var result = RegistrationNumber.Create(value);

        result.Value.Should().Be(new string('A', RegistrationNumber.MaxLength));
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = RegistrationNumber.Create("  J40/1234/2024  ");

        result.ToString().Should().Be("J40/1234/2024");
    }
}
