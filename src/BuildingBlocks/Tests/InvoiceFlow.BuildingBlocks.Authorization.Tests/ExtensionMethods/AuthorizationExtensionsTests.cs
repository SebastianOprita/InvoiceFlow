using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.ExtensionMethods;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class AuthorizationExtensionsTests
{
    [Fact]
    public void AddInvoiceFlowAuthorization_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddInvoiceFlowAuthorization();

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInvoiceFlowAuthorization_ShouldRegisterTenantScopeFilterAsScoped()
    {
        var services = new ServiceCollection();

        services.AddInvoiceFlowAuthorization();

        var descriptor = services.Single(
            x => x.ServiceType == typeof(TenantScopeFilter));

        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().Be(typeof(TenantScopeFilter));
    }

    [Fact]
    public void AddInvoiceFlowAuthorization_ShouldRegisterPermissionPolicyProviderAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInvoiceFlowAuthorization();

        // Assert
        var descriptor = services.Single(
            x =>
                x.ServiceType == typeof(IAuthorizationPolicyProvider) &&
                x.ImplementationType == typeof(PermissionPolicyProvider));

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInvoiceFlowAuthorization_ShouldRegisterPermissionAuthorizationHandlerAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInvoiceFlowAuthorization();

        // Assert
        var descriptor = services.Single(
            x =>
                x.ServiceType == typeof(IAuthorizationHandler) &&
                x.ImplementationType == typeof(PermissionAuthorizationHandler));

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInvoiceFlowAuthorization_ShouldRegisterExpectedPolicies()
    {
        var services = new ServiceCollection();

        services.AddInvoiceFlowAuthorization();

        using var provider = services.BuildServiceProvider();

        var options = provider
            .GetRequiredService<IOptions<AuthorizationOptions>>()
            .Value;

        options.GetPolicy(TenantUserLoginPolicy.TenantUserLogin)
            .Should()
            .NotBeNull();

        options.GetPolicy(PlatformUserLoginPolicy.PlatformUserLogin)
            .Should()
            .NotBeNull();
    }
}
