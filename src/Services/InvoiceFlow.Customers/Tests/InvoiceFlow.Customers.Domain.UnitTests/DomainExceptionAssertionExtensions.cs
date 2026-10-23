using FluentAssertions;
using FluentAssertions.Specialized;
using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain.UnitTests;

public static class DomainExceptionAssertionExtensions
{
    public static void WithDomainError(
        this ExceptionAssertions<DomainException> assertions,
        DomainError expectedError)
    {
        var exception = assertions.Which;

        exception.Error.Should().Be(expectedError);
        exception.Message.Should().Be($"{expectedError.ErrorCode}: {expectedError.ErrorMessage}");
    }
}
