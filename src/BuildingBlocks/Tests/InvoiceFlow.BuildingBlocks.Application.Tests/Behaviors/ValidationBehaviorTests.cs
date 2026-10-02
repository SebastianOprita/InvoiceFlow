using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Application.Tests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            Array.Empty<IValidator<TestRequest>>());

        var expectedResponse = new TestResponse();

        var nextCallCount = 0;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            nextCallCount++;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(
            new TestRequest(),
            next,
            CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
        nextCallCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithValidValidators_ShouldCallNext()
    {
        // Arrange
        var validator = new Mock<IValidator<TestRequest>>();

        validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [validator.Object]);

        var expectedResponse = new TestResponse();

        var nextCallCount = 0;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            nextCallCount++;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(
            new TestRequest(),
            next,
            CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
        nextCallCount.Should().Be(1);

        validator.Verify(
            x => x.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidationFailure_ShouldThrowValidationException()
    {
        // Arrange
        var failure = new ValidationFailure(
            "Name",
            "Name is required.");

        var validator = new Mock<IValidator<TestRequest>>();

        validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([failure]));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [validator.Object]);

        RequestHandlerDelegate<TestResponse> next =
            _ => Task.FromResult(new TestResponse());

        // Act
        var action = async () =>
            await behavior.Handle(
                new TestRequest(),
                next,
                CancellationToken.None);

        // Assert
        var exception = await action
            .Should()
            .ThrowAsync<ValidationException>();

        exception.Which.Errors
            .Should()
            .ContainSingle();

        exception.Which.Errors
            .Single()
            .Should()
            .BeSameAs(failure);
    }

    [Fact]
    public async Task Handle_WithMultipleValidationFailures_ShouldCombineFailures()
    {
        // Arrange
        var firstFailure = new ValidationFailure(
            "Name",
            "Name is required.");

        var secondFailure = new ValidationFailure(
            "Email",
            "Email is invalid.");

        var firstValidator = new Mock<IValidator<TestRequest>>();

        firstValidator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ValidationResult([firstFailure]));

        var secondValidator = new Mock<IValidator<TestRequest>>();

        secondValidator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ValidationResult([secondFailure]));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [
                firstValidator.Object,
                secondValidator.Object
            ]);

        // Act
        var action = async () =>
            await behavior.Handle(
                new TestRequest(),
                _ => Task.FromResult(new TestResponse()),
                CancellationToken.None);

        // Assert
        var exception = await action
            .Should()
            .ThrowAsync<ValidationException>();

        exception.Which.Errors
            .Should()
            .BeEquivalentTo([
                firstFailure,
                secondFailure
            ]);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldNotCallNext()
    {
        // Arrange
        var validator = new Mock<IValidator<TestRequest>>();

        validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ValidationResult([
                    new ValidationFailure(
                        "Name",
                        "Name is required.")
                ]));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [validator.Object]);

        var nextCallCount = 0;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            nextCallCount++;
            return Task.FromResult(new TestResponse());
        };

        // Act
        var action = async () =>
            await behavior.Handle(
                new TestRequest(),
                next,
                CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ValidationException>();

        nextCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToValidators()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;

        var validator = new Mock<IValidator<TestRequest>>();

        validator
            .Setup(x => x.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                cancellationToken))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [validator.Object]);

        // Act
        await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(new TestResponse()),
            cancellationToken);

        // Assert
        validator.Verify(
            x => x.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                cancellationToken),
            Times.Once);
    }

    public sealed record TestRequest;

    public sealed record TestResponse;
}