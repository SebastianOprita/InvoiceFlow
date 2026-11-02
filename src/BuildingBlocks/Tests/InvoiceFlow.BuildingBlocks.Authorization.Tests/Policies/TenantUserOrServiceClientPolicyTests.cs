using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class TenantUserOrServiceClientPolicyTests
{
    [Theory]
    [InlineData(PrincipalTypes.TenantUser, AuthenticationModes.Login)]
    [InlineData(PrincipalTypes.ServiceClient, AuthenticationModes.ClientCredentials)]
    public async Task Policy_ShouldAllowSupportedAuthenticatedPrincipal(
        string principalType, string authMode)
    {
        var result = await AuthorizeAsync(
            true,
            (InvoiceFlowClaimTypes.PrincipalType, principalType),
            (InvoiceFlowClaimTypes.AuthMode, authMode));

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(PrincipalTypes.TenantUser, AuthenticationModes.ClientCredentials)]
    [InlineData(PrincipalTypes.ServiceClient, AuthenticationModes.Login)]
    [InlineData(PrincipalTypes.PlatformUser, AuthenticationModes.Login)]
    public async Task Policy_ShouldRejectUnsupportedPrincipalAndModeCombinations(
        string principalType, string authMode)
    {
        var result = await AuthorizeAsync(
            true,
            (InvoiceFlowClaimTypes.PrincipalType, principalType),
            (InvoiceFlowClaimTypes.AuthMode, authMode));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Policy_ShouldRejectAnonymousPrincipalWithOtherwiseValidClaims()
    {
        var result = await AuthorizeAsync(
            false,
            (InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.TenantUser),
            (InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.Login));

        result.Succeeded.Should().BeFalse();
    }

    private static async Task<AuthorizationResult> AuthorizeAsync(
        bool authenticated,
        params (string Type, string Value)[] claims)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        using var provider = services.BuildServiceProvider();
        var identity = new System.Security.Claims.ClaimsIdentity(
            claims.Select(claim => new System.Security.Claims.Claim(claim.Type, claim.Value)),
            authenticated ? "test" : null);
        return await provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(
                new System.Security.Claims.ClaimsPrincipal(identity),
                null,
                TenantUserOrServiceClientPolicy.Build());
    }
}
