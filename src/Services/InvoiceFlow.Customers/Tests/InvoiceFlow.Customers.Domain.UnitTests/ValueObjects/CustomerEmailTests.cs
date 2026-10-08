using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Customers.Domain.UnitTests.ValueObjects;

public sealed class CustomerEmailTests
{
    [Fact]
    public void Create_ShouldReturnCustomerEmail_WhenValueIsValid()
    {
        var result = CustomerEmail.Create("john.doe@example.com");

        result.Value.Should().Be("john.doe@example.com");
        result.ToString().Should().Be("john.doe@example.com");
    }

    [Fact]
    public void Create_ShouldConvertValueToLowerCase()
    {
        var result = CustomerEmail.Create("John.Doe@Example.COM");

        result.Value.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = CustomerEmail.Create("  john.doe@example.com  ");

        result.Value.Should().Be("john.doe@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => CustomerEmail.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.EmailRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => CustomerEmail.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.EmailRequired);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var localPartLength = CustomerEmail.MaxLength - "@a.com".Length;
        var value = $"{new string('a', localPartLength)}@a.com";

        var result = CustomerEmail.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(CustomerEmail.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var localPartLength = CustomerEmail.MaxLength - "@a.com".Length + 1;
        var value = $"{new string('a', localPartLength)}@a.com";

        var act = () => CustomerEmail.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.EmailTooLong);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@example.com")]
    [InlineData("john.doe@")]
    [InlineData("john.doe")]
    [InlineData("john.doe.example.com")]
    public void Create_ShouldThrowDomainException_WhenEmailIsInvalid(string value)
    {
        var act = () => CustomerEmail.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.EmailInvalid);
    }

    [Fact]
    public void CreateOptional_ShouldReturnNull_WhenValueIsNull()
    {
        var result = CustomerEmail.CreateOptional(null);

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
        var result = CustomerEmail.CreateOptional(value);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_ShouldReturnCustomerEmail_WhenValueIsValid()
    {
        var result = CustomerEmail.CreateOptional("John.Doe@Example.COM");

        result.Should().NotBeNull();
        result!.Value.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void CreateOptional_ShouldTrimAndNormalizeValue()
    {
        var result = CustomerEmail.CreateOptional("  John.Doe@Example.COM  ");

        result.Should().NotBeNull();
        result!.Value.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void CreateOptional_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var localPartLength = CustomerEmail.MaxLength - "@a.com".Length + 1;
        var value = $"{new string('a', localPartLength)}@a.com";

        var act = () => CustomerEmail.CreateOptional(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.EmailTooLong);
    }

    [Fact]
    public void CreateOptional_ShouldThrowDomainException_WhenEmailIsInvalid()
    {
        var act = () => CustomerEmail.CreateOptional("invalid-email");

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.EmailInvalid);
    }

    [Fact]
    public void ToString_ShouldReturnNormalizedValue()
    {
        var result = CustomerEmail.Create("  John.Doe@Example.COM  ");

        result.ToString().Should().Be("john.doe@example.com");
    }
}

