using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class OrganizationNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateOrganizationName()
    {
        var organizationName = OrganizationName.Create("Acme Corporation");

        organizationName.Value.Should().Be("Acme Corporation");
        organizationName.ToString().Should().Be("Acme Corporation");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var organizationName = OrganizationName.Create("  Invoice Flow  ");

        organizationName.Value.Should().Be("Invoice Flow");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => OrganizationName.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.OrganizationNameRequired);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', OrganizationName.MaxLength + 1);

        var act = () => OrganizationName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.OrganizationNameInvalid);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateOrganizationName()
    {
        var value = new string('a', OrganizationName.MaxLength);

        var organizationName = OrganizationName.Create(value);

        organizationName.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var organizationName = OrganizationName.Create("Acme");

        organizationName.ToString().Should().Be("Acme");
    }
}
