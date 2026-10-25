using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _handler = new();

    [Fact]
    public async Task HandleAsync_WhenUserIsUnauthenticated_ShouldNotSucceed()
    {
        var requirement =
            new PermissionRequirement(SystemPermissions.CustomerRead);

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity());

        var context = new AuthorizationHandlerContext(
            [requirement],
            principal,
            resource: null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasRequiredPermission_ShouldSucceed()
    {
        var requirement =
            new PermissionRequirement(SystemPermissions.CustomerRead);

        var principal = CreateAuthenticatedPrincipal(
            SystemPermissions.CustomerRead);

        var context = new AuthorizationHandlerContext(
            [requirement],
            principal,
            resource: null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotHaveRequiredPermission_ShouldNotSucceed()
    {
        var requirement =
            new PermissionRequirement(SystemPermissions.CustomerStatusManage);

        var principal = CreateAuthenticatedPrincipal(
            SystemPermissions.CustomerRead);

        var context = new AuthorizationHandlerContext(
            [requirement],
            principal,
            resource: null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenRequirementContainsMultiplePermissionsAndUserHasAll_ShouldSucceed()
    {
        var required =
            SystemPermissions.CustomerRead |
            SystemPermissions.CustomerUpdate;

        var principal = CreateAuthenticatedPrincipal(required);

        var requirement = new PermissionRequirement(required);

        var context = new AuthorizationHandlerContext(
            [requirement],
            principal,
            resource: null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenRequirementContainsMultiplePermissionsAndUserHasOnlyOne_ShouldNotSucceed()
    {
        var requirement =
            new PermissionRequirement(
                SystemPermissions.CustomerRead |
                SystemPermissions.CustomerUpdate);

        var principal = CreateAuthenticatedPrincipal(
            SystemPermissions.CustomerRead);

        var context = new AuthorizationHandlerContext(
            [requirement],
            principal,
            resource: null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private static ClaimsPrincipal CreateAuthenticatedPrincipal(
        SystemPermissions permissions)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(
                InvoiceFlowClaimTypes.Permissions,
                ((long)permissions).ToString())
        ],
        authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }
}
