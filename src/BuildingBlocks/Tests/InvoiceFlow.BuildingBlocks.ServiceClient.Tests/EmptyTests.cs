using FluentAssertions;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.ServiceClient.Tests;

public sealed class EmptyTests
{
    [Fact]
    public void True()
    {
        var value = true;
        value.Should().BeTrue();
    }
}
