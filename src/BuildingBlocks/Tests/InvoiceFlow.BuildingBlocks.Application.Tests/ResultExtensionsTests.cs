using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Application.Tests;

public sealed class ResultExtensionsTests
{
    [Fact]
    public void ToActionResult_WithSuccessfulResult_ShouldReturnNoContent()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_WithSuccessfulGenericResult_ShouldReturnOkWithValue()
    {
        // Arrange
        var value = new TestResponse("Sebastian");

        var result = Result<TestResponse>.Success(value);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = actionResult
            .Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        okResult.Value.Should().BeSameAs(value);
    }

    [Theory]
    [InlineData(ApplicationErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ApplicationErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ApplicationErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ApplicationErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ApplicationErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ApplicationErrorType.Internal, StatusCodes.Status500InternalServerError)]
    public void ToActionResult_WithFailure_ShouldMapErrorTypeToExpectedStatusCode(
        ApplicationErrorType errorType,
        int expectedStatusCode)
    {
        // Arrange
        var error = new ApplicationError(
            errorType,
            "test.error",
            "Something went wrong.");

        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult
            .Should()
            .BeOfType<ObjectResult>()
            .Subject;

        objectResult.StatusCode.Should().Be(expectedStatusCode);

        var problem = objectResult.Value
            .Should()
            .BeOfType<ProblemDetails>()
            .Subject;

        problem.Status.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void ToActionResult_WithFailure_ShouldMapCodeToProblemDetailsTitle()
    {
        // Arrange
        var error = new ApplicationError(
            ApplicationErrorType.NotFound,
            "customer.not_found",
            "Customer was not found.");

        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var problem = GetProblemDetails(actionResult);

        problem.Title.Should().Be("customer.not_found");
    }

    [Fact]
    public void ToActionResult_WithNonInternalFailure_ShouldExposeErrorMessage()
    {
        // Arrange
        var error = new ApplicationError(
            ApplicationErrorType.Validation,
            "customer.invalid",
            "Customer is invalid.");

        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var problem = GetProblemDetails(actionResult);

        problem.Detail.Should().Be("Customer is invalid.");
    }

    [Fact]
    public void ToActionResult_WithInternalFailure_ShouldNotExposeInternalErrorMessage()
    {
        // Arrange
        var error = new ApplicationError(
            ApplicationErrorType.Internal,
            "internal.error",
            "SQL connection failed. Password=SuperSecret");

        var result = Result.Failure(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var problem = GetProblemDetails(actionResult);

        problem.Detail.Should().Be(
            "An internal server error occurred.");

        problem.Detail.Should().NotContain("SQL");
        problem.Detail.Should().NotContain("SuperSecret");
    }

    [Fact]
    public void ToActionResult_WithGenericFailure_ShouldReturnProblemDetails()
    {
        // Arrange
        var error = new ApplicationError(
            ApplicationErrorType.Conflict,
            "customer.conflict",
            "Customer already exists.");

        var result = Result<TestResponse>.Failure(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult
            .Should()
            .BeOfType<ObjectResult>()
            .Subject;

        objectResult.StatusCode
            .Should()
            .Be(StatusCodes.Status409Conflict);

        var problem = objectResult.Value
            .Should()
            .BeOfType<ProblemDetails>()
            .Subject;

        problem.Title.Should().Be("customer.conflict");
        problem.Detail.Should().Be("Customer already exists.");
        problem.Status.Should().Be(StatusCodes.Status409Conflict);
    }

    private static ProblemDetails GetProblemDetails(
        IActionResult actionResult)
    {
        var objectResult = actionResult
            .Should()
            .BeOfType<ObjectResult>()
            .Subject;

        return objectResult.Value
            .Should()
            .BeOfType<ProblemDetails>()
            .Subject;
    }

    public sealed record TestResponse(string Name);
}
