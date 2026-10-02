using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class PlatformUserLoginAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetPlatformUserLoginPolicy()
    {
        var attribute = new PlatformUserLoginAttribute();

        attribute.Policy
            .Should()
            .Be(PlatformUserLoginPolicy.PlatformUserLogin);
    }
}