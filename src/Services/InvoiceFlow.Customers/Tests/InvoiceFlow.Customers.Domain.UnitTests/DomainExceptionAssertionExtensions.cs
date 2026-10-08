using InvoiceFlow.BuildingBlocks.Domain;

namespace FluentAssertions;

public static class DomainExceptionAssertionExtensions
{
    public static void WithDomainError(
        this Specialized.ExceptionAssertions<DomainException> assertions,
        DomainError expectedError)
    {
        var exception = assertions.Which;

        exception.Error.Should().Be(expectedError);
        exception.Message.Should().Be($"{expectedError.ErrorCode}: {expectedError.ErrorMessage}");
    }
}
