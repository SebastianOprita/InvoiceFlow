using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class AddInvoiceLineCommandValidatorTests
{
    private readonly AddInvoiceLineCommandValidator _validator = new();
    private static readonly AddInvoiceLineCommand ValidCommand = new("request-key", Guid.CreateVersion7(), Guid.CreateVersion7(), "PROD-001", 2m, 10m);

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
        => _validator.TestValidate(ValidCommand).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_IdempotencyKey_Is_Empty(string? key)
        => _validator.TestValidate(ValidCommand with { IdempotencyKey = key! }).ShouldHaveValidationErrorFor(x => x.IdempotencyKey);

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
        => _validator.TestValidate(ValidCommand with { TenantId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.TenantId);

    [Fact]
    public void Should_Have_Error_When_InvoiceId_Is_Empty()
        => _validator.TestValidate(ValidCommand with { InvoiceId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.InvoiceId);

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_ProductCode_Is_Empty(string? code)
        => _validator.TestValidate(ValidCommand with { ProductCode = code! }).ShouldHaveValidationErrorFor(x => x.ProductCode);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Quantity_Is_Not_Positive(decimal quantity)
        => _validator.TestValidate(ValidCommand with { Quantity = quantity }).ShouldHaveValidationErrorFor(x => x.Quantity);

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Should_Have_Error_When_DiscountRate_Is_Out_Of_Range(decimal rate)
        => _validator.TestValidate(ValidCommand with { DiscountRate = rate }).ShouldHaveValidationErrorFor(x => x.DiscountRate);

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Should_Accept_DiscountRate_At_Bounds(decimal rate)
        => _validator.TestValidate(ValidCommand with { DiscountRate = rate }).ShouldNotHaveValidationErrorFor(x => x.DiscountRate);

    [Fact]
    public void Should_Accept_Null_DiscountRate()
        => _validator.TestValidate(ValidCommand with { DiscountRate = null }).ShouldNotHaveValidationErrorFor(x => x.DiscountRate);
}
