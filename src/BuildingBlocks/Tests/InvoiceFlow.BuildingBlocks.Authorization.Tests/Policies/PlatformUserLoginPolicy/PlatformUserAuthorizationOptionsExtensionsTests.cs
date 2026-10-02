using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class PlatformUserAuthorizationOptionsExtensionsTests
{
    [Fact]
    public void AddPlatformUserLoginPolicy_ShouldRegisterPolicy()
    {
        var options = new AuthorizationOptions();

        var result = options.AddPlatformUserLoginPolicy();

        var policy = options.GetPolicy(
            PlatformUserLoginPolicy.PlatformUserLogin);

        result.Should().BeSameAs(options);
        policy.Should().NotBeNull();
    }
}