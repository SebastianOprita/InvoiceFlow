using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Api.Tests;

public sealed class OpenTelemetryExtensionsTests
{
    [Fact]
    public void AddInvoiceFlowObservability_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddInvoiceFlowObservability(
            "InvoiceFlow.Test");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddInvoiceFlowObservability_ShouldRegisterTracerProvider()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddLogging();

        builder.AddInvoiceFlowObservability(
            "InvoiceFlow.Test");

        using var serviceProvider =
            builder.Services.BuildServiceProvider();

        // Act
        var tracerProvider =
            serviceProvider.GetService<TracerProvider>();

        // Assert
        tracerProvider.Should().NotBeNull();
    }

    [Fact]
    public void AddInvoiceFlowObservability_ShouldBuildServiceProviderSuccessfully()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddLogging();

        builder.AddInvoiceFlowObservability(
            "InvoiceFlow.Test");

        // Act
        var action = () =>
            builder.Services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateScopes = true,
                    ValidateOnBuild = true
                });

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void AddInvoiceFlowObservability_ShouldListenToMassTransitActivitySource()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddLogging();

        builder.AddInvoiceFlowObservability(
            "InvoiceFlow.Test");

        using var serviceProvider =
            builder.Services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<TracerProvider>();

        using var activitySource =
            new ActivitySource("MassTransit");

        // Act
        using var activity =
            activitySource.StartActivity("TestActivity");

        // Assert
        activity.Should().NotBeNull();
    }

    [Fact]
    public void AddInvoiceFlowObservability_ShouldListenToBuildingBlocksApplicationActivitySource()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddLogging();

        builder.AddInvoiceFlowObservability(
            "InvoiceFlow.Test");

        using var serviceProvider =
            builder.Services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<TracerProvider>();

        using var activitySource =
            new ActivitySource("InvoiceFlow.BuildingBlocks.Application");

        // Act
        using var activity =
            activitySource.StartActivity("TestActivity");

        // Assert
        activity.Should().NotBeNull();
    }
}