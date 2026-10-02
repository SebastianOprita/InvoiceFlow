using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class AuthorizationOptionsExtensionsTests
{
    [Fact]
    public void AddTenantUserLoginPolicy_ShouldRegisterPolicy()
    {
        // Arrange
        var options = new AuthorizationOptions();

        // Act
        var result = options.AddTenantUserLoginPolicy();

        // Assert
        result.Should().BeSameAs(options);

        options.GetPolicy(TenantUserLoginPolicy.TenantUserLogin)
            .Should()
            .NotBeNull();
    }
}
