using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using System.Security.Claims;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_WithValidClaim_ShouldReturnUserId()
    {
        var userId = Guid.CreateVersion7();

        var principal = CreatePrincipal(
            new Claim(InvoiceFlowClaimTypes.UserId, userId.ToString()));

        principal.GetUserId().Should().Be(userId);
    }

    [Fact]
    public void GetUserId_WithMissingClaim_ShouldReturnNull()
    {
        var principal = CreatePrincipal();

        principal.GetUserId().Should().BeNull();
    }

    [Fact]
    public void GetUserId_WithInvalidClaim_ShouldReturnNull()
    {
        var principal = CreatePrincipal(
            new Claim(InvoiceFlowClaimTypes.UserId, "not-a-guid"));

        principal.GetUserId().Should().BeNull();
    }

    [Fact]
    public void GetTenantId_WithValidClaim_ShouldReturnTenantId()
    {
        var tenantId = Guid.CreateVersion7();

        var principal = CreatePrincipal(
            new Claim(InvoiceFlowClaimTypes.TenantId, tenantId.ToString()));

        principal.GetTenantId().Should().Be(tenantId);
    }

    [Fact]
    public void GetPermissions_WithMissingClaim_ShouldReturnNone()
    {
        var principal = CreatePrincipal();

        principal.GetPermissions().Should().Be(SystemPermission.None);
    }

    [Fact]
    public void GetPermissions_WithInvalidValue_ShouldReturnNone()
    {
        var principal = CreatePrincipal(
            new Claim(InvoiceFlowClaimTypes.Permissions, "invalid"));

        principal.GetPermissions().Should().Be(SystemPermission.None);
    }

    [Fact]
    public void GetPermissions_WithValidPermissions_ShouldReturnPermissions()
    {
        var permissions =
            SystemPermission.CustomerView |
            SystemPermission.CustomerCreate;

        var principal = CreatePrincipal(
            new Claim(
                InvoiceFlowClaimTypes.Permissions,
                ((long)permissions).ToString()));

        principal.GetPermissions().Should().Be(permissions);
    }

    [Fact]
    public void HasPermission_WhenPermissionExists_ShouldReturnTrue()
    {
        var permissions =
            SystemPermission.CustomerView |
            SystemPermission.CustomerCreate;

        var principal = CreatePrincipal(
            new Claim(
                InvoiceFlowClaimTypes.Permissions,
                ((long)permissions).ToString()));

        principal
            .HasPermission(SystemPermission.CustomerView)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void HasPermission_WhenPermissionDoesNotExist_ShouldReturnFalse()
    {
        var principal = CreatePrincipal(
            new Claim(
                InvoiceFlowClaimTypes.Permissions,
                ((long)SystemPermission.CustomerView).ToString()));

        principal
            .HasPermission(SystemPermission.CustomerDelete)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsImpersonating_WithImpersonationMode_ShouldReturnTrue()
    {
        var principal = CreatePrincipal(
            new Claim(
                InvoiceFlowClaimTypes.AuthMode,
                AuthenticationModes.Impersonation));

        principal.IsImpersonating().Should().BeTrue();
    }

    [Theory]
    [InlineData(AuthenticationModes.Login)]
    [InlineData("")]
    [InlineData("something-else")]
    public void IsImpersonating_WithNonImpersonationMode_ShouldReturnFalse(
        string authMode)
    {
        var principal = CreatePrincipal(
            new Claim(
                InvoiceFlowClaimTypes.AuthMode,
                authMode));

        principal.IsImpersonating().Should().BeFalse();
    }

    [Fact]
    public void IsImpersonating_WithMissingClaim_ShouldReturnFalse()
    {
        var principal = CreatePrincipal();

        principal.IsImpersonating().Should().BeFalse();
    }

    private static ClaimsPrincipal CreatePrincipal(
        params Claim[] claims)
    {
        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }
}
