using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class ServiceClientNameTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateServiceClientName()
    {
        var serviceClientName = ServiceClientName.Create("Billing Service");

        serviceClientName.Value.Should().Be("Billing Service");
        serviceClientName.ToString().Should().Be("Billing Service");
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var serviceClientName = ServiceClientName.Create("  Invoice Flow  ");

        serviceClientName.Value.Should().Be("Invoice Flow");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => ServiceClientName.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ServiceClientNameRequired);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', ServiceClientName.MaxLength + 1);

        var act = () => ServiceClientName.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ServiceClientNameInvalid);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateServiceClientName()
    {
        var value = new string('a', ServiceClientName.MaxLength);

        var serviceClientName = ServiceClientName.Create(value);

        serviceClientName.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var serviceClientName = ServiceClientName.Create("Billing Service");

        serviceClientName.ToString().Should().Be("Billing Service");
    }
}
