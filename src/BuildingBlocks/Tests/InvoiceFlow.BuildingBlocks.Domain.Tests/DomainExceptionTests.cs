using FluentAssertions;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Domain.Tests;

public sealed class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithErrorCodeAndMessage_ShouldSetProperties()
    {
        // Arrange
        const string errorCode = "customer.invalid";
        const string errorMessage = "Customer is invalid.";

        // Act
        var exception = new DomainException(errorCode, errorMessage);

        // Assert
        exception.Error.ErrorCode.Should().Be(errorCode);
        exception.Error.ErrorMessage.Should().Be(errorMessage);
        exception.Message.Should().Be($"{errorCode}: {errorMessage}");
    }

    [Fact]
    public void Constructor_WithTuple_ShouldSetProperties()
    {
        // Arrange
        var error = (
            errorCode: "customer.invalid",
            errorMessage: "Customer is invalid.");

        // Act
        var exception = new DomainException(error);

        // Assert
        exception.Error.ErrorCode.Should().Be(error.errorCode);
        exception.Error.ErrorMessage.Should().Be(error.errorMessage);
        exception.Message.Should().Be($"{error.errorCode}: {error.errorMessage}");
    }

    [Fact]
    public void Constructor_WithDomainError_ShouldSetProperties()
    {
        // Arrange
        var error = new DomainError(
            "customer.invalid",
            "Customer is invalid.");

        // Act
        var exception = new DomainException(error);

        // Assert
        exception.Error.ErrorCode.Should().Be(error.ErrorCode);
        exception.Error.ErrorMessage.Should().Be(error.ErrorMessage);
        exception.Message.Should().Be($"{error.ErrorCode}: {error.ErrorMessage}");
    }
}
