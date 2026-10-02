using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class TenantUserLoginPolicyTests
{
    [Fact]
    public void Build_ShouldRequireAuthenticatedUser()
    {
        var policy = TenantUserLoginPolicy.Build();

        policy.Requirements
            .OfType<DenyAnonymousAuthorizationRequirement>()
            .Should()
            .ContainSingle();
    }

    [Fact]
    public void Build_ShouldRequireTenantUserPrincipalType()
    {
        var policy = TenantUserLoginPolicy.Build();

        var requirement = policy.Requirements
            .OfType<ClaimsAuthorizationRequirement>()
            .Single(x => x.ClaimType == InvoiceFlowClaimTypes.PrincipalType);

        requirement.AllowedValues
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(PrincipalTypes.TenantUser);
    }

    [Fact]
    public void Build_ShouldRequireLoginAuthenticationMode()
    {
        var policy = TenantUserLoginPolicy.Build();

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
