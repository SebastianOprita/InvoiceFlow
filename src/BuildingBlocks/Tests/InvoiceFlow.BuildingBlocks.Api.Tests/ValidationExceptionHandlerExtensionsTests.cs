using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Api.Tests;

public sealed class ValidationExceptionHandlerExtensionsTests
{
    [Fact]
    public async Task ValidationException_ShouldReturnBadRequest()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        var app = builder.Build();

        app.UseValidationExceptionHandler();

        app.Run(_ =>
        {
            throw new ValidationException(
            [
                new ValidationFailure("Name", "Name is required."),
                new ValidationFailure("Name", "Name is too short."),
                new ValidationFailure("Email", "Email is invalid.")
            ]);
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(content);

        document.RootElement
            .GetProperty("message")
            .GetString()
            .Should()
            .Be("Validation failed.");

        var errors = document.RootElement.GetProperty("errors");

        errors.GetProperty("Name")
            .EnumerateArray()
            .Select(x => x.GetString())
            .Should()
            .BeEquivalentTo(
                "Name is required.",
                "Name is too short.");

        errors.GetProperty("Email")
            .EnumerateArray()
            .Select(x => x.GetString())
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be("Email is invalid.");

        await app.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UnexpectedException_ShouldReturnInternalServerError()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        var app = builder.Build();

        app.UseValidationExceptionHandler();

        app.Run(_ =>
        {
            throw new InvalidOperationException("Boom");
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(content);

        document.RootElement
            .GetProperty("message")
            .GetString()
            .Should()
            .Be("An unexpected error occurred.");

        await app.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void UseValidationExceptionHandler_ShouldReturnSameApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();

        var appBuilder = new ApplicationBuilder(services);

        // Act
        var result = appBuilder.UseValidationExceptionHandler();

        // Assert
        result.Should().BeSameAs(appBuilder);
    }
}
