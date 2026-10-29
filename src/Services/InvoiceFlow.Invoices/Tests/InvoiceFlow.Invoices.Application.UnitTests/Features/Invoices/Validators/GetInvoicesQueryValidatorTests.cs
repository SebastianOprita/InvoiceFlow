using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class GetInvoicesQueryValidatorTests
{
    private readonly GetInvoicesQueryValidator _validator = new();
    private readonly GetInvoicesQuery _validQuery = new(InvoiceTestData.TenantId);

    [Fact]
    public void Should_Not_Have_Errors_When_Query_Is_Valid()
    {
        var result = _validator.TestValidate(_validQuery);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        var query = _validQuery with { TenantId = Guid.Empty };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }
}
