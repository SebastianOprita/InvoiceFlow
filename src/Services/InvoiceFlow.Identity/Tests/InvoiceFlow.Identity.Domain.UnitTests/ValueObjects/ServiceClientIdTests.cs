using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class ServiceClientIdTests
{
    [Theory]
    [InlineData("billing-service")]
    [InlineData("service_client-2")]
    [InlineData("client123")]
    public void Create_WithValidValue_ShouldCreateServiceClientId(string value)
    {
        var serviceClientId = ServiceClientIdentifier.Create(value);

        serviceClientId.Value.Should().Be(value);
        serviceClientId.ToString().Should().Be(value);
    }

    [Fact]
    public void Create_ShouldTrimValue()
    {
        var serviceClientId = ServiceClientIdentifier.Create("  billing-service  ");

        serviceClientId.Value.Should().Be("billing-service");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpace_ShouldThrowDomainException(string? value)
    {
        var act = () => ServiceClientIdentifier.Create(value!);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ServiceClientIdRequired);
    }

    [Theory]
    [InlineData("Billing-service")]
    [InlineData("billing--service")]
    [InlineData("_billing")]
    [InlineData("billing-")]
    [InlineData("billing service")]
    [InlineData("billing.service")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string value)
    {
        var act = () => ServiceClientIdentifier.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ServiceClientIdInvalid);
    }

    [Fact]
    public void Create_WithValueLongerThanMaxLength_ShouldThrowDomainException()
    {
        var value = new string('a', ServiceClientIdentifier.MaxLength + 1);

        var act = () => ServiceClientIdentifier.Create(value);

        act.Should()
            .Throw<DomainException>()
            .WithDomainError(DomainErrors.ServiceClientIdInvalid);
    }

    [Fact]
    public void Create_WithValueEqualToMaxLength_ShouldCreateServiceClientId()
    {
        var value = new string('a', ServiceClientIdentifier.MaxLength);

        var serviceClientId = ServiceClientIdentifier.Create(value);

        serviceClientId.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var serviceClientId = ServiceClientIdentifier.Create("billing-service");

        serviceClientId.ToString().Should().Be("billing-service");
    }
}
