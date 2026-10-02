using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Authorization.ExtensionMethods;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Authorization.Tests;

public sealed class AuthenticationExtensionsTests
{
    [Fact]
    public void AddInvoiceFlowAuthentication_WithMissingJwtSettings_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        // Act
        var action = () =>
            services.AddInvoiceFlowAuthentication(configuration);

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("JwtSettings is missing.");
    }

    [Fact]
    public void AddInvoiceFlowAuthentication_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        var result = services.AddInvoiceFlowAuthentication(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInvoiceFlowAuthentication_ShouldConfigureBearerAsDefaultScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddInvoiceFlowAuthentication(configuration);

        using var provider = services.BuildServiceProvider();

        var options = provider
            .GetRequiredService<IOptions<AuthenticationOptions>>()
            .Value;

        // Assert
        options.DefaultScheme
            .Should()
            .Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task AddInvoiceFlowAuthentication_ShouldConfigureBearerAsDefaultAuthenticateScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddInvoiceFlowAuthentication(configuration);

        using var provider = services.BuildServiceProvider();

        var schemeProvider =
            provider.GetRequiredService<IAuthenticationSchemeProvider>();

        // Act
        var scheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();

        // Assert
        scheme.Should().NotBeNull();
        scheme!.Name.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void AddInvoiceFlowAuthentication_ShouldConfigureJwtValidationParameters()
    {
        // Arrange
        const string issuer = "InvoiceFlow.Identity";
        const string audience = "InvoiceFlow.Api";
        const string secret = "this-is-a-test-secret-with-enough-length-123456";

        var services = new ServiceCollection();

        var configuration = CreateConfiguration(
            issuer,
            audience,
            secret);

        services.AddInvoiceFlowAuthentication(configuration);

        using var provider = services.BuildServiceProvider();

        var optionsMonitor =
            provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();

        var options = optionsMonitor.Get(
            JwtBearerDefaults.AuthenticationScheme);

        var validation = options.TokenValidationParameters;

        // Assert
        validation.ValidateIssuer.Should().BeTrue();
        validation.ValidIssuer.Should().Be(issuer);

        validation.ValidateAudience.Should().BeTrue();
        validation.ValidAudience.Should().Be(audience);

        validation.ValidateIssuerSigningKey.Should().BeTrue();

        validation.ValidateLifetime.Should().BeTrue();

        validation.ClockSkew.Should().Be(TimeSpan.Zero);

        validation.IssuerSigningKey
            .Should()
            .BeOfType<SymmetricSecurityKey>();

        var signingKey =
            (SymmetricSecurityKey)validation.IssuerSigningKey;

        signingKey.Key
            .Should()
            .Equal(Encoding.UTF8.GetBytes(secret));
    }

    private static IConfiguration CreateConfiguration(
        string issuer = "InvoiceFlow.Identity",
        string audience = "InvoiceFlow.Api",
        string secret = "this-is-a-test-secret-with-enough-length-123456")
    {
        var values = new Dictionary<string, string?>
        {
            ["JwtSettings:Issuer"] = issuer,
            ["JwtSettings:Audience"] = audience,
            ["JwtSettings:Secret"] = secret
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
