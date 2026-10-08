using FluentAssertions;
using MediatR;
using System.Diagnostics;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Application.Tests;

public sealed class TracingBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnResponseFromNext()
    {
        // Arrange
        var behavior = CreateBehavior();
        var expectedResponse = new TestResponse();

        RequestHandlerDelegate<TestResponse> next =
            _ => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.Handle(
            new TestRequest(),
            next,
            TestContext.Current.CancellationToken);

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
            TestContext.Current.CancellationToken);

        // Assert
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldCreateActivityWithCorrectNameAndKind()
    {
        // Arrange
        using var listener = CreateListener();

        Activity? capturedActivity = null;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            capturedActivity = Activity.Current;
            return Task.FromResult(new TestResponse());
        };

        // Act
        await CreateBehavior().Handle(
            new TestRequest(),
            next,
            TestContext.Current.CancellationToken);

        // Assert
        capturedActivity.Should().NotBeNull();

        capturedActivity!.DisplayName
            .Should().Be(nameof(TestRequest));

        capturedActivity.Kind
            .Should().Be(ActivityKind.Internal);

        capturedActivity.Source.Name
            .Should().Be(ApplicationDiagnostics.ActivitySourceName);

        capturedActivity.Duration
            .Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_WithParentActivity_ShouldPreserveTraceId()
    {
        // Arrange
        using var listener = CreateListener();

        using var parent = new Activity("Parent");
        parent.Start();

        var expectedTraceId = parent.TraceId;

        Activity? childActivity = null;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            childActivity = Activity.Current;
            return Task.FromResult(new TestResponse());
        };

        // Act
        await CreateBehavior().Handle(
            new TestRequest(),
            next,
            TestContext.Current.CancellationToken);

        // Assert
        childActivity.Should().NotBeNull();

        childActivity!.TraceId
            .Should().Be(expectedTraceId);

        childActivity.ParentSpanId
            .Should().Be(parent.SpanId);
    }

    [Fact]
    public async Task Handle_ShouldSetRequestTags()
    {
        // Arrange
        using var listener = CreateListener();

        Activity? capturedActivity = null;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            capturedActivity = Activity.Current;
            return Task.FromResult(new TestResponse());
        };

        // Act
        await CreateBehavior().Handle(
            new TestRequest(),
            next,
            TestContext.Current.CancellationToken);

        // Assert
        capturedActivity.Should().NotBeNull();

        capturedActivity!.GetTagItem("mediatr.request.name")
            .Should().Be(nameof(TestRequest));

        capturedActivity.GetTagItem("mediatr.request.type")
            .Should().Be(typeof(TestRequest).FullName);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldSetActivityStatusOk()
    {
        // Arrange
        using var listener = CreateListener();

        Activity? capturedActivity = null;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            capturedActivity = Activity.Current;
            return Task.FromResult(new TestResponse());
        };

        // Act
        await CreateBehavior().Handle(
            new TestRequest(),
            next,
            TestContext.Current.CancellationToken);

        // Assert
        capturedActivity.Should().NotBeNull();

        capturedActivity!.Status
            .Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldPropagateException()
    {
        // Arrange
        using var listener = CreateListener();

        var expectedException =
            new InvalidOperationException("Boom");

        RequestHandlerDelegate<TestResponse> next =
            _ => throw expectedException;

        // Act
        var action = async () =>
            await CreateBehavior().Handle(
                new TestRequest(),
                next,
                TestContext.Current.CancellationToken);

        // Assert
        var assertion = await action.Should()
            .ThrowAsync<InvalidOperationException>();

        assertion.Which.Should().BeSameAs(expectedException);
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldMarkActivityAsError()
    {
        // Arrange
        using var listener = CreateListener();

        Activity? capturedActivity = null;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            capturedActivity = Activity.Current;
            throw new InvalidOperationException("Boom");
        };

        // Act
        var action = async () =>
            await CreateBehavior().Handle(
                new TestRequest(),
                next,
                TestContext.Current.CancellationToken);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>();

        capturedActivity.Should().NotBeNull();

        capturedActivity!.Status
            .Should().Be(ActivityStatusCode.Error);

        capturedActivity.StatusDescription
            .Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldRecordExceptionEvent()
    {
        // Arrange
        using var listener = CreateListener();

        Activity? capturedActivity = null;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            capturedActivity = Activity.Current;
            throw new InvalidOperationException("Boom");
        };

        // Act
        var action = async () =>
            await CreateBehavior().Handle(
                new TestRequest(),
                next,
                TestContext.Current.CancellationToken);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>();

        capturedActivity.Should().NotBeNull();

        var exceptionEvent = capturedActivity!.Events
            .Single(x => x.Name == "exception");

        exceptionEvent.Tags
            .Should()
            .Contain(x =>
                x.Key == "exception.type" &&
                Equals(x.Value, typeof(InvalidOperationException).FullName));
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldNotMarkActivityAsError()
    {
        // Arrange
        using var listener = CreateListener();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        Activity? capturedActivity = null;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            capturedActivity = Activity.Current;

            throw new OperationCanceledException(
                cancellationTokenSource.Token);
        };

        // Act
        var action = async () =>
            await CreateBehavior().Handle(
                new TestRequest(),
                next,
                cancellationTokenSource.Token);

        // Assert
        await action.Should()
            .ThrowAsync<OperationCanceledException>();

        capturedActivity.Should().NotBeNull();

        capturedActivity!.Status
            .Should().NotBe(ActivityStatusCode.Error);

        capturedActivity.GetTagItem("mediatr.request.cancelled")
            .Should().Be(true);
    }

    [Fact]
    public async Task Handle_ShouldRestoreParentActivityAfterExecution()
    {
        // Arrange
        using var listener = CreateListener();

        using var parent = new Activity("Parent");
        parent.Start();

        var behavior = CreateBehavior();

        // Act
        await behavior.Handle(
            new TestRequest(),
            _ => Task.FromResult(new TestResponse()),
            TestContext.Current.CancellationToken);

        // Assert
        Activity.Current.Should().BeSameAs(parent);
    }

    private static TracingBehavior<TestRequest, TestResponse> CreateBehavior()
    {
        return new TracingBehavior<TestRequest, TestResponse>();
    }

    private static ActivityListener CreateListener()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source =>
                source.Name == ApplicationDiagnostics.ActivitySourceName,

            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,

            SampleUsingParentId =
                (ref ActivityCreationOptions<string> _) =>
                    ActivitySamplingResult.AllDataAndRecorded
        };

        ActivitySource.AddActivityListener(listener);

        return listener;
    }

    public sealed record TestRequest;

    public sealed record TestResponse;
}
