using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Products.Domain.UnitTests.ValueObjects;

public sealed class ProductDescriptionTests
{
    [Fact]
    public void Create_ShouldReturnProductDescription_WhenValueIsValid()
    {
        var result = ProductDescription.Create("A useful product description.");

        result.Value.Should().Be("A useful product description.");
        result.ToString().Should().Be("A useful product description.");
    }

    [Fact]
    public void Create_ShouldTrimValue_WhenValueHasLeadingOrTrailingWhitespace()
    {
        var result = ProductDescription.Create("  A useful product description.  ");

        result.Value.Should().Be("A useful product description.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_ShouldThrowDomainException_WhenValueIsWhiteSpace(string value)
    {
        var act = () => ProductDescription.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductDescriptionRequired);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNull()
    {
        var act = () => ProductDescription.Create(null!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductDescriptionRequired);
    }

    [Fact]
    public void Create_ShouldAllowValue_WhenLengthIsExactlyMaxLength()
    {
        var value = new string('A', ProductDescription.MaxLength);

        var result = ProductDescription.Create(value);

        result.Value.Should().Be(value);
        result.Value.Length.Should().Be(ProductDescription.MaxLength);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', ProductDescription.MaxLength + 1);

        var act = () => ProductDescription.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductDescriptionTooLong);
    }

    [Fact]
    public void CreateOptional_ShouldReturnNull_WhenValueIsNull()
    {
        var result = ProductDescription.CreateOptional(null);

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
        var result = ProductDescription.CreateOptional(value);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_ShouldReturnProductDescription_WhenValueIsValid()
    {
        var result = ProductDescription.CreateOptional("A useful product description.");

        result.Should().NotBeNull();
        result!.Value.Should().Be("A useful product description.");
    }

    [Fact]
    public void CreateOptional_ShouldTrimValue()
    {
        var result = ProductDescription.CreateOptional("  A useful product description.  ");

        result.Should().NotBeNull();
        result!.Value.Should().Be("A useful product description.");
    }

    [Fact]
    public void CreateOptional_ShouldThrowDomainException_WhenValueExceedsMaxLength()
    {
        var value = new string('A', ProductDescription.MaxLength + 1);

        var act = () => ProductDescription.CreateOptional(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ProductDescriptionTooLong);
    }

    [Fact]
    public void ToString_ShouldReturnTrimmedValue()
    {
        var result = ProductDescription.Create("  A useful product description.  ");

        result.ToString().Should().Be("A useful product description.");
    }
}
