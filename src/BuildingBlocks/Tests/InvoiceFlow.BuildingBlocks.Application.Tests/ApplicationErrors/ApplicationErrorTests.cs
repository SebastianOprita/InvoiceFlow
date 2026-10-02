using FluentAssertions;
using Xunit;

namespace InvoiceFlow.BuildingBlocks.Application.Tests;

public sealed class ApplicationErrorTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var error = new ApplicationError(
            ApplicationErrorType.Validation,
            "customer.invalid",
            "Customer is invalid.");

        error.Type.Should().Be(ApplicationErrorType.Validation);
        error.Code.Should().Be("customer.invalid");
        error.Message.Should().Be("Customer is invalid.");
    }
}
