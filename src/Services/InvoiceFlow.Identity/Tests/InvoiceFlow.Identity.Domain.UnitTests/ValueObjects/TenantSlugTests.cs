using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class TenantSlugTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateTenantSlug()
    {
        var slug = TenantSlug.Create("invoice-flow");

        slug.Value.Should().Be("invoice-flow");
        slug.ToString().Should().Be("invoice-flow");
    }

    [Fact]
    public void Create_ShouldTrimAndNormalizeToLowercase()
    {
        var slug = TenantSlug.Create("  Invoice-Flow  ");

        slug.Value.Should().Be("invoice-flow");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => TenantSlug.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.SlugRequired.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', TenantSlug.MaxLength + 1);

        var act = () => TenantSlug.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.SlugTooLong.ErrorMessage);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateTenantSlug()
    {
        var value = new string('a', TenantSlug.MaxLength);

        var slug = TenantSlug.Create(value);

        slug.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("-tenant")]
    [InlineData("tenant-")]
    public void Create_WithLeadingOrTrailingDash_ShouldThrowDomainException(string value)
    {
        var act = () => TenantSlug.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.SlugStartsOrEndsInvalid.ErrorMessage);
    }

    [Theory]
    [InlineData("tenant_slug")]
    [InlineData("tenant slug")]
    [InlineData("tenant.slug")]
    [InlineData("tenant@slug")]
    [InlineData("tenant!")]
    public void Create_WithInvalidCharacters_ShouldThrowDomainException(string value)
    {
        var act = () => TenantSlug.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(DomainErrors.SlugInvalid.ErrorMessage);
    }

    [Theory]
    [InlineData("tenant")]
    [InlineData("tenant-123")]
    [InlineData("abc")]
    [InlineData("company-2026")]
    public void Create_WithValidCharacters_ShouldCreateTenantSlug(string value)
    {
        var slug = TenantSlug.Create(value);

        slug.Value.Should().Be(value.ToLowerInvariant());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var slug = TenantSlug.Create("my-tenant");

        slug.ToString().Should().Be("my-tenant");
    }
}
