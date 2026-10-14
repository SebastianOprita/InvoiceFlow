using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class PermissionPolicyTests
{
    [Fact]
    public void CreateName_ShouldReturnExpectedPolicyName()
    {
        var permission =
            SystemPermissions.CustomerRead |
            SystemPermissions.CustomerUpdate;

        var result = PermissionPolicy.CreateName(permission);

        result.Should().Be($"Permission:{(long)permission}");
    }
}
