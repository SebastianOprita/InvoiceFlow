using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Identity.Domain;
using Xunit;

namespace InvoiceFlow.Identity.Domain.UnitTests.ValueObjects;

public sealed class ServiceClientSecretHashTests
{
    [Fact]
    public void Create_NormalizesHexHashToUppercase()
    {
        ServiceClientSecretHash.Create(new string('a', 64)).Value.Should().Be(new string('A', 64));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    public void Create_RejectsMissingOrMalformedHash(string value) =>
        FluentActions.Invoking(() => ServiceClientSecretHash.Create(value)).Should().Throw<DomainException>();
}
