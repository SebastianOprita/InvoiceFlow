using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Invoices.Application.UnitTests;

public sealed class UpsertInvoicePaymentSummaryCommandValidatorTests
{
    private readonly UpsertInvoicePaymentSummaryCommandValidator _sut = new();

    private static readonly UpsertInvoicePaymentSummaryCommand Valid = new(
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        25m,
        75m,
        "RON",
        1,
        DateTimeOffset.Parse("2025-01-01T10:00:00Z"),
        DateTimeOffset.Parse("2025-01-01T10:01:00Z"));

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
    public void Rejects_negative_paid_amount() =>
        _sut.TestValidate(Valid with { PaidAmount = -0.01m })
            .ShouldHaveValidationErrorFor(x => x.PaidAmount);

    [Fact]
    public void Rejects_negative_outstanding_amount() =>
        _sut.TestValidate(Valid with { OutstandingAmount = -0.01m })
            .ShouldHaveValidationErrorFor(x => x.OutstandingAmount);

    [Theory]
    [InlineData("")]
    [InlineData("RO")]
    [InlineData("RONX")]
    public void Requires_three_character_currency_code(string value) =>
        _sut.TestValidate(Valid with { CurrencyCode = value })
            .ShouldHaveValidationErrorFor(x => x.CurrencyCode);

    [Fact]
    public void Rejects_negative_payment_balance_version() =>
        _sut.TestValidate(Valid with { PaymentBalanceVersion = -1 })
            .ShouldHaveValidationErrorFor(x => x.PaymentBalanceVersion);

    [Fact]
    public void Requires_source_updated_timestamp() =>
        _sut.TestValidate(Valid with { SourceUpdatedAtUtc = default })
            .ShouldHaveValidationErrorFor(x => x.SourceUpdatedAtUtc);

    [Fact]
    public void Requires_applied_timestamp() =>
        _sut.TestValidate(Valid with { AppliedAtUtc = default })
            .ShouldHaveValidationErrorFor(x => x.AppliedAtUtc);
}
