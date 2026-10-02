using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class TenantScopeFilterTests
{
    private readonly Mock<ILogger<TenantScopeFilter>> _logger = new();

    [Fact]
    public async Task OnResourceExecutionAsync_WhenTenantIdIsMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var sut = new TenantScopeFilter(_logger.Object);

        var context = CreateContext();

        var nextCalled = false;

        ResourceExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        };

        // Act
        await sut.OnResourceExecutionAsync(context, next);

        // Assert
        context.Result
            .Should()
            .BeOfType<BadRequestObjectResult>();

        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task OnResourceExecutionAsync_WhenTenantIdIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var sut = new TenantScopeFilter(_logger.Object);

        var context = CreateContext(
            tenantId: "invalid-tenant-id");

        var nextCalled = false;

        ResourceExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        };

        // Act
        await sut.OnResourceExecutionAsync(context, next);

        // Assert
        context.Result
            .Should()
            .BeOfType<BadRequestObjectResult>();

        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task OnResourceExecutionAsync_WhenUserIsAnonymous_ShouldCallNext()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var sut = new TenantScopeFilter(_logger.Object);

        var context = CreateContext(
            tenantId: tenantId.ToString(),
            principal: new ClaimsPrincipal(
                new ClaimsIdentity()));

        var nextCalled = false;

        ResourceExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        };

        // Act
        await sut.OnResourceExecutionAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnResourceExecutionAsync_WhenAuthenticatedUserHasMissingTenantClaim_ShouldForbid()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var principal = CreateAuthenticatedPrincipal();

        var sut = new TenantScopeFilter(_logger.Object);

        var context = CreateContext(
            tenantId: tenantId.ToString(),
            principal: principal);

        var nextCalled = false;

        ResourceExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        };

        // Act
        await sut.OnResourceExecutionAsync(context, next);

        // Assert
        context.Result
            .Should()
            .BeOfType<ForbidResult>();

        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task OnResourceExecutionAsync_WhenAuthenticatedUserHasInvalidTenantClaim_ShouldForbid()
    {
        // Arrange
        var routeTenantId = Guid.CreateVersion7();

        var principal = CreateAuthenticatedPrincipal(
            new Claim(
                InvoiceFlowClaimTypes.TenantId,
                "not-a-guid"));

        var sut = new TenantScopeFilter(_logger.Object);

        var context = CreateContext(
            tenantId: routeTenantId.ToString(),
            principal: principal);

        var nextCalled = false;

        ResourceExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        };

        // Act
        await sut.OnResourceExecutionAsync(context, next);

        // Assert
        context.Result
            .Should()
            .BeOfType<ForbidResult>();

        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task OnResourceExecutionAsync_WhenRouteTenantDoesNotMatchTokenTenant_ShouldForbid()
    {
        // Arrange
        var routeTenantId = Guid.CreateVersion7();
        var tokenTenantId = Guid.CreateVersion7();

        var principal = CreateAuthenticatedPrincipal(
            new Claim(
                InvoiceFlowClaimTypes.TenantId,
                tokenTenantId.ToString()));

        var sut = new TenantScopeFilter(_logger.Object);

        var context = CreateContext(
            tenantId: routeTenantId.ToString(),
            principal: principal);

        var nextCalled = false;

        ResourceExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        };

        // Act
        await sut.OnResourceExecutionAsync(context, next);

        // Assert
        context.Result
            .Should()
            .BeOfType<ForbidResult>();

        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task OnResourceExecutionAsync_WhenRouteTenantMatchesTokenTenant_ShouldCallNext()
    {
        // Arrange
        var tenantId = Guid.CreateVersion7();

        var principal = CreateAuthenticatedPrincipal(
            new Claim(
                InvoiceFlowClaimTypes.TenantId,
                tenantId.ToString()));

        var sut = new TenantScopeFilter(_logger.Object);

        var context = CreateContext(
            tenantId: tenantId.ToString(),
            principal: principal);

        var nextCalled = false;

        ResourceExecutionDelegate next = () =>
        {
            nextCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        };

        // Act
        await sut.OnResourceExecutionAsync(context, next);

        // Assert
        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public void TenantScopedAttribute_ShouldUseTenantScopeFilter()
    {
        var attribute = new TenantScopedAttribute();

        attribute.ImplementationType
            .Should()
            .Be(typeof(TenantScopeFilter));
    }

    private static ResourceExecutingContext CreateContext(
        string? tenantId = null,
        ClaimsPrincipal? principal = null)
    {
        var httpContext = new DefaultHttpContext
        {
            User = principal ?? new ClaimsPrincipal()
        };

        var routeData = new RouteData();

        if (tenantId is not null)
        {
            routeData.Values["tenantId"] = tenantId;
        }

        var actionContext = new ActionContext(
            httpContext,
            routeData,
            new ActionDescriptor());

        return new ResourceExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new List<IValueProviderFactory>());
    }

    private static ResourceExecutedContext CreateExecutedContext(
        ResourceExecutingContext context)
    {
        return new ResourceExecutedContext(
            context,
            new List<IFilterMetadata>());
    }

    private static ClaimsPrincipal CreateAuthenticatedPrincipal(
        params Claim[] claims)
    {
        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }
}
