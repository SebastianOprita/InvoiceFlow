using FluentAssertions;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Application.Tests;

public sealed class ResultOfTTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        var result = Result<string>.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        var error = new ApplicationError(
            ApplicationErrorType.Validation,
            "customer.invalid",
            "Customer is invalid.");

        var result = Result<string>.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeSameAs(error);
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Failure_WithFailedResult_ShouldCopyError()
    {
        var error = new ApplicationError(
            ApplicationErrorType.Internal,
            "internal.error",
            "Something went wrong.");

        var failedResult = Result.Failure(error);

        var result = Result<int>.Failure(failedResult);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(error);
        result.Value.Should().Be(default);
    }

    [Fact]
    public void Failure_WithSuccessfulResult_ShouldThrow()
    {
        var successfulResult = Result.Success();

        var action = () => Result<int>.Failure(successfulResult);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Cannot create a failure result from a successful result.");
    }
}
