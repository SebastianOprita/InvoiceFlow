using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class TenantUserLoginAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetTenantUserLoginPolicy()
    {
        var attribute = new TenantUserLoginAttribute();

        attribute.Policy
            .Should()
            .Be(TenantUserLoginPolicy.TenantUserLogin);
    }
}