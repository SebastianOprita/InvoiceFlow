using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Products.Domain.UnitTests.ValueObjects;

public sealed class ProductNameTests
{
    [Fact]
    public void Create_ShouldReturnProductName_WhenValueIsValid()
    {
        var result = ProductName.Create("Product name");

        result.Value.Should().Be("Product name");
        result.ToString().Should().Be("Product name");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = ProductName.Create("  Product name  ");

        result.Value.Should().Be("Product name");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => ProductName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductNameRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => ProductName.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductNameRequired);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', ProductName.MaxLength);

        var result = ProductName.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(ProductName.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', ProductName.MaxLength + 1);

        var act = () => ProductName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductNameTooLong);
    }

    [Fact]
    public void ToString_ShouldReturnTrimmedValue()
    {
        var result = ProductName.Create("  Product name  ");

        result.ToString().Should().Be("Product name");
    }
}
