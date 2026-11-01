using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class ServiceClientSecretDescriptionTests
{
    [Fact]
    public void Create_TrimsAndStoresValue()
    {
        ServiceClientSecretDescription.Create("  production  ").Value.Should().Be("production");
    }

    [Fact]
    public void CreateOptional_ReturnsNullForMissingOrWhitespace()
    {
        ServiceClientSecretDescription.CreateOptional(null).Should().BeNull();
        ServiceClientSecretDescription.CreateOptional("   ").Should().BeNull();
    }

    [Fact]
    public void Create_RejectsBlankAndTooLongValues()
    {
        FluentActions.Invoking(() => ServiceClientSecretDescription.Create(" ")).Should().Throw<DomainException>();
        FluentActions.Invoking(() => ServiceClientSecretDescription.Create(new string('x', ServiceClientSecretDescription.MaxLength + 1))).Should().Throw<DomainException>();
    }
}
