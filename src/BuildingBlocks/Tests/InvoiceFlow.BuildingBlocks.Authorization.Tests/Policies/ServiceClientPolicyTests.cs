using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class ServiceClientPolicyTests
{
    [Fact]
    public async Task Policy_ShouldAcceptAuthenticatedServiceClientCredentials()
    {
        var result = await AuthorizeAsync(
            ServiceClientPolicy.Build(),
            true,
            (InvoiceFlowClaimTypes.PrincipalType, PrincipalTypes.ServiceClient),
            (InvoiceFlowClaimTypes.AuthMode, AuthenticationModes.ClientCredentials));

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(PrincipalTypes.TenantUser, AuthenticationModes.ClientCredentials)]
    [InlineData(PrincipalTypes.ServiceClient, AuthenticationModes.Login)]
    [InlineData(PrincipalTypes.ServiceClient, AuthenticationModes.ClientCredentials, false)]
    public async Task Policy_ShouldRejectWrongPrincipalModeOrAnonymous(
        string principalType, string authMode, bool authenticated = true)
    {
        var result = await AuthorizeAsync(
            ServiceClientPolicy.Build(),
            authenticated,
            (InvoiceFlowClaimTypes.PrincipalType, principalType),
            (InvoiceFlowClaimTypes.AuthMode, authMode));

        result.Succeeded.Should().BeFalse();
    }

    private static async Task<AuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
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
            .AuthorizeAsync(new System.Security.Claims.ClaimsPrincipal(identity), null, policy);
    }
}
