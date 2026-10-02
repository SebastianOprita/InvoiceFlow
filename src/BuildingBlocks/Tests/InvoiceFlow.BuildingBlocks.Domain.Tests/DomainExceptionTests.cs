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
        exception.ErrorCode.Should().Be(errorCode);
        exception.ErrorMessage.Should().Be(errorMessage);
        exception.Message.Should().Be(errorMessage);
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
        exception.ErrorCode.Should().Be(error.errorCode);
        exception.ErrorMessage.Should().Be(error.errorMessage);
        exception.Message.Should().Be(error.errorMessage);
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
        exception.ErrorCode.Should().Be(error.ErrorCode);
        exception.ErrorMessage.Should().Be(error.ErrorMessage);
        exception.Message.Should().Be(error.ErrorMessage);
    }
}
