using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Api.Tests;

public sealed class OpenTelemetryExtensionsTests
{
    [Fact]
    public void AddInvoiceFlowTracing_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddInvoiceFlowTracing(
            "InvoiceFlow.Test");

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInvoiceFlowTracing_ShouldRegisterTracerProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddInvoiceFlowTracing(
            "InvoiceFlow.Test");

        using var serviceProvider =
            services.BuildServiceProvider();

        // Act
        var tracerProvider =
            serviceProvider.GetService<TracerProvider>();

        // Assert
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddInvoiceFlowTracing_ShouldBuildServiceProviderSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddInvoiceFlowTracing(
            "InvoiceFlow.Test");

        // Act
        var action = () =>
            services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateScopes = true,
                    ValidateOnBuild = true
                });

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddInvoiceFlowTracing_ShouldListenToMassTransitActivitySource()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddInvoiceFlowTracing(
            "InvoiceFlow.Test");

        using var serviceProvider =
            services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<TracerProvider>();

        using var activitySource =
            new ActivitySource("MassTransit");

        // Act
        using var activity =
            activitySource.StartActivity("TestActivity");

        // Assert
        activity.Should().NotBeNull();
    }
}