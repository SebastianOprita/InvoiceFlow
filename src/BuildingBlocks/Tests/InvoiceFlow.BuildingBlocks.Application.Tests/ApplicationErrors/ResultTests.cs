using FluentAssertions;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Application.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        var error = new ApplicationError(
            ApplicationErrorType.NotFound,
            "customer.not_found",
            "Customer was not found.");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(error);
    }

    [Fact]
    public void Failure_WithFailedResult_ShouldCopyError()
    {
        var error = new ApplicationError(
            ApplicationErrorType.Conflict,
            "customer.conflict",
            "Customer conflict.");

        var failedResult = Result.Failure(error);

        var result = Result.Failure(failedResult);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(error);
    }

    [Fact]
    public void Failure_WithSuccessfulResult_ShouldThrow()
    {
        var successfulResult = Result.Success();

        var action = () => Result.Failure(successfulResult);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot create a failure result from a successful result.");
    }
}

