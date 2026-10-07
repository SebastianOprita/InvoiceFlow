using System.Diagnostics;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Application.Tests;

public sealed class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>> _logger = new();
    private readonly Mock<ISystemDateTimeProvider> _dateTimeProvider = new();

    [Fact]
    public async Task Handle_ShouldReturnResponseFromNext()
    {
        // Arrange
        var behavior = CreateBehavior();

        var request = new TestRequest();
        var expectedResponse = new TestResponse();

        RequestHandlerDelegate<TestResponse> next =
            _ => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.Handle(
            request,
            next,
            CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    [Fact]
    public async Task Handle_ShouldCallNextExactlyOnce()
    {
        // Arrange
        var behavior = CreateBehavior();

        var callCount = 0;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            callCount++;
            return Task.FromResult(new TestResponse());
        };

        // Act
        await behavior.Handle(
            new TestRequest(),
            next,
            CancellationToken.None);

        // Assert
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithCurrentActivity_ShouldUseActivityTraceId()
    {
        // Arrange
        using var activity = new Activity("test");
        activity.Start();

        var expectedTraceId = activity.TraceId.ToString();

        Dictionary<string, object?>? capturedScope = null;

        _logger
            .Setup(x => x.BeginScope(
                It.IsAny<Dictionary<string, object?>>()))
            .Callback((Dictionary<string, object?> scope) =>
                capturedScope = scope);

        var behavior = CreateBehavior();

        // Act
        await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(new TestResponse()),
            CancellationToken.None);

        // Assert
        capturedScope.Should().NotBeNull();

        capturedScope!["TraceId"]
            .Should()
            .Be(expectedTraceId);
    }

    [Fact]
    public async Task Handle_WithoutCurrentActivity_ShouldUseNullTraceId()
    {
        // Arrange
        Activity.Current = null;

        Dictionary<string, object?>? capturedScope = null;

        _logger
            .Setup(x => x.BeginScope(
                It.IsAny<Dictionary<string, object?>>()))
            .Callback((Dictionary<string, object?> scope) =>
                capturedScope = scope);

        var behavior = CreateBehavior();

        // Act
        await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(new TestResponse()),
            CancellationToken.None);

        // Assert
        capturedScope.Should().NotBeNull();

        capturedScope!.Should().ContainKey("TraceId");
        capturedScope["TraceId"].Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldPropagateException()
    {
        // Arrange
        var behavior = CreateBehavior();

        RequestHandlerDelegate<TestResponse> next =
            _ => throw new InvalidOperationException("Boom");

        // Act
        var action = async () =>
            await behavior.Handle(
                new TestRequest(),
                next,
                CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Boom");
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldStillLogCompletion()
    {
        // Arrange
        var behavior = CreateBehavior();

        RequestHandlerDelegate<TestResponse> next =
            _ => throw new InvalidOperationException("Boom");

        // Act
        var action = async () =>
            await behavior.Handle(
                new TestRequest(),
                next,
                CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>();

        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state != null &&
                    state.ToString()!.Contains("Handled TestRequest")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogProvidedDateTime()
    {
        // Arrange
        var now = new DateTime(
            2026,
            10,
            2,
            12,
            30,
            0,
            DateTimeKind.Utc);

        _dateTimeProvider
            .Setup(x => x.Now)
            .Returns(now);

        var behavior = CreateBehavior();

        // Act
        await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(new TestResponse()),
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state != null &&
                    HasLogProperty(state, "DateTimeUtc", now) &&
                    HasLogProperty(
                        state,
                        "RequestName",
                        nameof(TestRequest))),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private LoggingBehavior<TestRequest, TestResponse> CreateBehavior()
    {
        return new LoggingBehavior<TestRequest, TestResponse>(
            _logger.Object,
            _dateTimeProvider.Object);
    }

    private static bool HasLogProperty(
        object state,
        string propertyName,
        object expectedValue)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> properties)
            return false;

        return properties.Any(x =>
            x.Key == propertyName &&
            Equals(x.Value, expectedValue));
    }

    public sealed record TestRequest;

    public sealed record TestResponse;
}
