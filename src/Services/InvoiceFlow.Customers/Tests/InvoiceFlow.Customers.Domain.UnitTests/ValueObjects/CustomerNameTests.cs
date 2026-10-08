using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class CustomerNameTests
{
    [Fact]
    public void Create_ShouldReturnCustomerName_WhenValueIsValid()
    {
        var result = CustomerName.Create("Acme Corporation");

        result.Value.Should().Be("Acme Corporation");
        result.ToString().Should().Be("Acme Corporation");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = CustomerName.Create("  Acme Corporation  ");

        result.Value.Should().Be("Acme Corporation");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => CustomerName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CustomerNameRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => CustomerName.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CustomerNameRequired);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', CustomerName.MaxLength);

        var result = CustomerName.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(CustomerName.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', CustomerName.MaxLength + 1);

        var act = () => CustomerName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.CustomerNameTooLong);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', CustomerName.MaxLength)}  ";

        var result = CustomerName.Create(value);

        result.Value.Should().Be(new string('A', CustomerName.MaxLength));
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = CustomerName.Create("  Acme Corporation  ");

        result.ToString().Should().Be("Acme Corporation");
    }
}
