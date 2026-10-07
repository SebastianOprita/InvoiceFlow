using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using System.Text.Json;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Api.Tests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task UnexpectedException_ShouldReturnInternalServerError()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Services.AddInvoiceFlowExceptionHandling();

        var app = builder.Build();

        app.UseExceptionHandler();

        app.Run(_ =>
        {
            throw new InvalidOperationException("Boom");
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync(
            "/",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(content);

        document.RootElement
            .GetProperty("message")
            .GetString()
            .Should()
            .Be("An unexpected error occurred.");

        var errorId = document.RootElement
            .GetProperty("errorId")
            .GetString();

        errorId
            .Should()
            .NotBeNullOrWhiteSpace();

        Guid.TryParseExact(errorId, "N", out _)
            .Should()
            .BeTrue();

        await app.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidationException_ShouldBeHandledByValidationHandler()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Services.AddInvoiceFlowExceptionHandling();

        var app = builder.Build();

        app.UseExceptionHandler();

        app.Run(_ =>
        {
            throw new ValidationException(
            [
                new ValidationFailure("Name", "Name is required.")
            ]);
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync(
            "/",
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(content);

        document.RootElement.TryGetProperty("errorId", out _)
            .Should()
            .BeFalse();

        await app.StopAsync(TestContext.Current.CancellationToken);
    }
}
