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

public sealed class ValidationExceptionHandlerTests
{
    [Fact]
    public async Task ValidationException_ShouldReturnBadRequest()
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
                new ValidationFailure("Name", "Name is required."),
                new ValidationFailure("Name", "Name is too short."),
                new ValidationFailure("Email", "Email is invalid.")
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
}
