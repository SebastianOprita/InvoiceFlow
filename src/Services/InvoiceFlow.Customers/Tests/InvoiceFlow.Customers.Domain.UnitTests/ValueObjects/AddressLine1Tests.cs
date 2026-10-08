using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class AddressLine1Tests
{
    [Fact]
    public void Create_ShouldReturnAddressLine1_WhenValueIsValid()
    {
        var result = AddressLine1.Create("123 Main Street");

        result.Value.Should().Be("123 Main Street");
        result.ToString().Should().Be("123 Main Street");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = AddressLine1.Create("  123 Main Street  ");

        result.Value.Should().Be("123 Main Street");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsNullOrWhiteSpace(string value)
    {
        var act = () => AddressLine1.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.AddressLine1Required);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => AddressLine1.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.AddressLine1Required);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', AddressLine1.MaxLength);

        var result = AddressLine1.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(AddressLine1.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', AddressLine1.MaxLength + 1);

        var act = () => AddressLine1.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.AddressLine1TooLong);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', AddressLine1.MaxLength)}  ";

        var result = AddressLine1.Create(value);

        result.Value.Should().Be(new string('A', AddressLine1.MaxLength));
    }
}
