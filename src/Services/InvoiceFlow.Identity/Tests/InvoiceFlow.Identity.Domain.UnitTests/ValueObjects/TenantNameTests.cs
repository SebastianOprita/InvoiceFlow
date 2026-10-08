using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class TenantNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateTenantName()
    {
        var tenantName = TenantName.Create("Acme Corporation");

        tenantName.Value.Should().Be("Acme Corporation");
        tenantName.ToString().Should().Be("Acme Corporation");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var tenantName = TenantName.Create("  Invoice Flow  ");

        tenantName.Value.Should().Be("Invoice Flow");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => TenantName.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.NameRequired);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', TenantName.MaxLength + 1);

        var act = () => TenantName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.NameTooLong);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateTenantName()
    {
        var value = new string('a', TenantName.MaxLength);

        var tenantName = TenantName.Create(value);

        tenantName.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var tenantName = TenantName.Create("Tenant A");

        tenantName.ToString().Should().Be("Tenant A");
    }
}
