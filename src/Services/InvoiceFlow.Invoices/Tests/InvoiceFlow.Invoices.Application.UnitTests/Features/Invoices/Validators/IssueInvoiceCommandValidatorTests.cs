using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class IssueInvoiceCommandValidatorTests
{
    private readonly IssueInvoiceCommandValidator _validator = new();
    private static readonly IssueInvoiceCommand ValidCommand = new(Guid.CreateVersion7(), Guid.CreateVersion7());

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
        => _validator.TestValidate(ValidCommand).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
        => _validator.TestValidate(ValidCommand with { TenantId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.TenantId);

    [Fact]
    public void Should_Have_Error_When_InvoiceId_Is_Empty()
        => _validator.TestValidate(ValidCommand with { InvoiceId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.InvoiceId);
}
