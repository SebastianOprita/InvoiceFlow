using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class CreateInvoiceCommandValidatorTests
{
    private readonly CreateInvoiceCommandValidator _validator = new();
    private static readonly CreateInvoiceCommand ValidCommand = new("request-key", Guid.CreateVersion7(), "CUST-001", "Invoice notes");

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
        => _validator.TestValidate(ValidCommand).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_IdempotencyKey_Is_Empty(string? key)
        => _validator.TestValidate(ValidCommand with { IdempotencyKey = key! }).ShouldHaveValidationErrorFor(x => x.IdempotencyKey);

    [Fact]
    public void Should_Have_Error_When_IdempotencyKey_Exceeds_Max_Length()
        => _validator.TestValidate(ValidCommand with { IdempotencyKey = new string('k', 129) }).ShouldHaveValidationErrorFor(x => x.IdempotencyKey);

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
        => _validator.TestValidate(ValidCommand with { TenantId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.TenantId);

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_CustomerCode_Is_Empty(string? code)
        => _validator.TestValidate(ValidCommand with { CustomerCode = code! }).ShouldHaveValidationErrorFor(x => x.CustomerCode);

    [Fact]
    public void Should_Have_Error_When_CustomerCode_Exceeds_Max_Length()
        => _validator.TestValidate(ValidCommand with { CustomerCode = new string('c', 39) }).ShouldHaveValidationErrorFor(x => x.CustomerCode);

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Notes_Are_Empty(string? notes)
        => _validator.TestValidate(ValidCommand with { Notes = notes! }).ShouldHaveValidationErrorFor(x => x.Notes);

    [Fact]
    public void Should_Have_Error_When_Notes_Exceed_Max_Length()
        => _validator.TestValidate(ValidCommand with { Notes = new string('n', 2001) }).ShouldHaveValidationErrorFor(x => x.Notes);
}
