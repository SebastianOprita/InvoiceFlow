using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Api.Tests;

public sealed class ExceptionHandlerExtensionsTests
{
    [Fact]
    public void AddInvoiceFlowExceptionHandling_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddInvoiceFlowExceptionHandling();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInvoiceFlowExceptionHandling_ShouldRegisterExceptionHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddInvoiceFlowExceptionHandling();

        var provider = services.BuildServiceProvider();

        // Act
        var handlers = provider
            .GetServices<IExceptionHandler>()
            .ToArray();

        // Assert
        handlers.Should().HaveCount(2);

        handlers[0]
            .Should()
            .BeOfType<ValidationExceptionHandler>();

        handlers[1]
            .Should()
            .BeOfType<GlobalExceptionHandler>();
    }
}
