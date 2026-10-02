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
            SystemPermission.CustomerView |
            SystemPermission.CustomerUpdate;

        var result = PermissionPolicy.CreateName(permission);

        result.Should().Be($"Permission:{(long)permission}");
    }
}
