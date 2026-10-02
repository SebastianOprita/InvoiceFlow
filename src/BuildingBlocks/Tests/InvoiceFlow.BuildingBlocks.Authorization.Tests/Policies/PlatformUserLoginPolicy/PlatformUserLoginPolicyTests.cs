using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class PlatformUserLoginPolicyTests
{
    [Fact]
    public void Build_ShouldRequireAuthenticatedUser()
    {
        var policy = PlatformUserLoginPolicy.Build();

        policy.Requirements
            .OfType<DenyAnonymousAuthorizationRequirement>()
            .Should()
            .ContainSingle();
    }

    [Fact]
    public void Build_ShouldRequirePlatformUserPrincipalType()
    {
        var policy = PlatformUserLoginPolicy.Build();

        var requirement = policy.Requirements
            .OfType<ClaimsAuthorizationRequirement>()
            .Single(x => x.ClaimType == InvoiceFlowClaimTypes.PrincipalType);

        requirement.AllowedValues
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(PrincipalTypes.PlatformUser);
    }

    [Fact]
    public void Build_ShouldRequireLoginAuthenticationMode()
    {
        var policy = PlatformUserLoginPolicy.Build();

        var requirement = policy.Requirements
            .OfType<ClaimsAuthorizationRequirement>()
            .Single(x => x.ClaimType == InvoiceFlowClaimTypes.AuthMode);

        requirement.AllowedValues
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(AuthenticationModes.Login);
    }
}
