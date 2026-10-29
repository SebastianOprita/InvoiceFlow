using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class GetInvoiceByIdQueryValidatorTests
{
    private readonly GetInvoiceByIdQueryValidator _validator = new();
    private readonly GetInvoiceByIdQuery _validQuery = new(InvoiceTestData.TenantId, Guid.CreateVersion7());

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

    [Fact]
    public void Should_Have_Error_When_InvoiceId_Is_Empty()
    {
        var query = _validQuery with { InvoiceId = Guid.Empty };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.InvoiceId);
    }
}
