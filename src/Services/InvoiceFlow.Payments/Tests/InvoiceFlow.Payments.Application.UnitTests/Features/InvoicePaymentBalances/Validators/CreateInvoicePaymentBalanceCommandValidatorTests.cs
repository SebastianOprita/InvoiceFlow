using FluentValidation.TestHelper;
using InvoiceFlow.Payments.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class CreateInvoicePaymentBalanceCommandValidatorTests
{
    private readonly CreateInvoicePaymentBalanceCommandValidator _sut = new();

    private static readonly CreateInvoicePaymentBalanceCommand Valid = new(
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        "INV-001",
        "RON",
        125.50m,
        DateTimeOffset.Parse("2025-01-01T10:00:00Z"));

    [Fact]
    public void Accepts_valid_command() =>
        _sut.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Requires_tenant_id() =>
        _sut.TestValidate(Valid with { TenantId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.TenantId);

    [Fact]
    public void Requires_invoice_id() =>
        _sut.TestValidate(Valid with { InvoiceId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.InvoiceId);

    [Fact]
    public void Requires_customer_id() =>
        _sut.TestValidate(Valid with { CustomerId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.CustomerId);

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Requires_invoice_number(string? value) =>
        _sut.TestValidate(Valid with { InvoiceNumber = value! })
            .ShouldHaveValidationErrorFor(x => x.InvoiceNumber);

    [Fact]
    public void Limits_invoice_number_length() =>
        _sut.TestValidate(Valid with { InvoiceNumber = new string('I', InvoiceNumber.MaxLength + 1) })
            .ShouldHaveValidationErrorFor(x => x.InvoiceNumber);

    [Theory]
    [InlineData("")]
    [InlineData("ZZZ")]
    public void Requires_supported_currency(string value) =>
        _sut.TestValidate(Valid with { CurrencyCode = value })
            .ShouldHaveValidationErrorFor(x => x.CurrencyCode);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Requires_positive_total_amount(decimal value) =>
        _sut.TestValidate(Valid with { TotalAmount = value })
            .ShouldHaveValidationErrorFor(x => x.TotalAmount);

    [Fact]
    public void Rejects_amount_with_excess_currency_precision() =>
        _sut.TestValidate(Valid with { TotalAmount = 1.234m })
            .ShouldHaveValidationErrorFor(x => x.TotalAmount);

    [Fact]
    public void Requires_created_at() =>
        _sut.TestValidate(Valid with { CreatedAtUtc = default })
            .ShouldHaveValidationErrorFor(x => x.CreatedAtUtc);
}
