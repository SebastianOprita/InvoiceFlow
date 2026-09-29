using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class CustomerCodeTests
{
    [Fact]
    public void Create_ShouldReturnCustomerCode_WhenValueIsValid()
    {
        var result = CustomerCode.Create("CUST-001");

        result.Value.Should().Be("CUST-001");
        result.ToString().Should().Be("CUST-001");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = CustomerCode.Create("  CUST-001  ");

        result.Value.Should().Be("CUST-001");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => CustomerCode.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.CustomerCodeRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => CustomerCode.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.CustomerCodeRequired.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', CustomerCode.MaxLength);

        var result = CustomerCode.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(CustomerCode.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', CustomerCode.MaxLength + 1);

        var act = () => CustomerCode.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.CustomerCodeTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_ShouldValidateLengthAfterTrimming()
    {
        var value = $"  {new string('A', CustomerCode.MaxLength)}  ";

        var result = CustomerCode.Create(value);

        result.Value.Should().Be(new string('A', CustomerCode.MaxLength));
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = CustomerCode.Create("  CUST-001  ");

        result.ToString().Should().Be("CUST-001");
    }
}
