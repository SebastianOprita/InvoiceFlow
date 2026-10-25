using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Products.Domain.UnitTests.ValueObjects;

public sealed class ProductCodeTests
{
    [Fact]
    public void Create_ShouldReturnProductCode_WhenValueIsValid()
    {
        var result = ProductCode.Create("PROD-001");

        result.Value.Should().Be("PROD-001");
        result.ToString().Should().Be("PROD-001");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = ProductCode.Create("  PROD-001  ");

        result.Value.Should().Be("PROD-001");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => ProductCode.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductCodeRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => ProductCode.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductCodeRequired);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', ProductCode.MaxLength);

        var result = ProductCode.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(ProductCode.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', ProductCode.MaxLength + 1);

        var act = () => ProductCode.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductCodeTooLong);
    }

    [Fact]
    public void CreateOptional_ShouldReturnNull_WhenValueIsNull()
    {
        var result = ProductCode.CreateOptional(null);

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
        var result = ProductCode.CreateOptional(value);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_ShouldReturnProductCode_WhenValueIsValid()
    {
        var result = ProductCode.CreateOptional("PROD-001");

        result.Should().NotBeNull();
        result!.Value.Should().Be("PROD-001");
    }

    [Fact]
    public void CreateOptional_ShouldTrimValue()
    {
        var result = ProductCode.CreateOptional("  PROD-001  ");

        result.Should().NotBeNull();
        result!.Value.Should().Be("PROD-001");
    }

    [Fact]
    public void CreateOptional_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', ProductCode.MaxLength + 1);

        var act = () => ProductCode.CreateOptional(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductCodeTooLong);
    }

    [Fact]
    public void ToString_ShouldReturnTrimmedValue()
    {
        var result = ProductCode.Create("  PROD-001  ");

        result.ToString().Should().Be("PROD-001");
    }
}
