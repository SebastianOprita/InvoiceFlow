using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class AddressLine2Tests
{
    [Fact]
    public void Create_ShouldReturnAddressLine2_WhenValueIsValid()
    {
        var result = AddressLine2.Create("Apartment 12B");

        result.Value.Should().Be("Apartment 12B");
        result.ToString().Should().Be("Apartment 12B");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = AddressLine2.Create("  Apartment 12B  ");

        result.Value.Should().Be("Apartment 12B");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => AddressLine2.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.AddressLine2Required);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => AddressLine2.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.AddressLine2Required);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', AddressLine2.MaxLength);

        var result = AddressLine2.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(AddressLine2.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', AddressLine2.MaxLength + 1);

        var act = () => AddressLine2.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.AddressLine2TooLong);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', AddressLine2.MaxLength)}  ";

        var result = AddressLine2.Create(value);

        result.Value.Should().Be(new string('A', AddressLine2.MaxLength));
    }

    [Fact]
    public void CreateOptional_ShouldReturnNull_WhenValueIsNull()
    {
        var result = AddressLine2.CreateOptional(null);

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
        var result = AddressLine2.CreateOptional(value);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_ShouldReturnAddressLine2_WhenValueIsValid()
    {
        var result = AddressLine2.CreateOptional("Apartment 12B");

        result.Should().NotBeNull();
        result!.Value.Should().Be("Apartment 12B");
    }

    [Fact]
    public void CreateOptional_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = AddressLine2.CreateOptional("  Apartment 12B  ");

        result.Should().NotBeNull();
        result!.Value.Should().Be("Apartment 12B");
    }

    [Fact]
    public void CreateOptional_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', AddressLine2.MaxLength + 1);

        var act = () => AddressLine2.CreateOptional(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.AddressLine2TooLong);
    }
}
