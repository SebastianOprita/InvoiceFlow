using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class HasPermissionAttributeTests
{
    [Fact]
    public void Constructor_ShouldSetExpectedPolicy()
    {
        var attribute =
            new HasPermissionAttribute(SystemPermission.CustomerView);

        attribute.Policy
            .Should()
            .Be(PermissionPolicy.CreateName(
                SystemPermission.CustomerView));
    }
}
