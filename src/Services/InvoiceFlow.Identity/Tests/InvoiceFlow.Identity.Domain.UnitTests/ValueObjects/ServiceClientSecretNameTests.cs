using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class ServiceClientSecretNameTests
{
    [Fact]
    public void Create_TrimsAndStoresValue()
    {
        var name = ServiceClientSecretName.Create("  deploy key  ");
        name.Value.Should().Be("deploy key");
        name.ToString().Should().Be("deploy key");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankName(string value) =>
        FluentActions.Invoking(() => ServiceClientSecretName.Create(value)).Should().Throw<DomainException>();

    [Fact]
    public void Create_RejectsTooLongName() =>
        FluentActions.Invoking(() => ServiceClientSecretName.Create(new string('x', ServiceClientSecretName.MaxLength + 1))).Should().Throw<DomainException>();
}
