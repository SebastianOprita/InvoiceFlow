using FluentValidation.TestHelper;
using InvoiceFlow.Payments.Application.Features.Payments.Commands.CreatePayment;
using InvoiceFlow.Payments.Domain;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class CreatePaymentCommandValidatorTests
{
    private readonly CreatePaymentCommandValidator _sut = new();
    private static readonly CreatePaymentCommand Valid = new("key", Guid.CreateVersion7(), "CUST-001", "RON", 12.34m, PaymentMethod.Card, "ref", "notes");

    [Fact] public void Valid_command_has_no_errors() => _sut.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();
    [Theory, InlineData(""), InlineData(null)] public void Requires_idempotency_key(string? value) => _sut.TestValidate(Valid with { IdempotencyKey = value! }).ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    [Fact] public void Limits_idempotency_key() => _sut.TestValidate(Valid with { IdempotencyKey = new string('k', 129) }).ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    [Fact] public void Requires_tenant() => _sut.TestValidate(Valid with { TenantId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.TenantId);
    [Theory, InlineData(""), InlineData(null)] public void Requires_customer_code(string? value) => _sut.TestValidate(Valid with { CustomerCode = value! }).ShouldHaveValidationErrorFor(x => x.CustomerCode);
    [Fact] public void Limits_customer_code() => _sut.TestValidate(Valid with { CustomerCode = new string('c', 37) }).ShouldHaveValidationErrorFor(x => x.CustomerCode);
    [Theory, InlineData(""), InlineData("ZZZ")] public void Requires_supported_currency(string value) => _sut.TestValidate(Valid with { CurrencyCode = value }).ShouldHaveValidationErrorFor(x => x.CurrencyCode);
    [Theory, InlineData(0), InlineData(-1)] public void Requires_positive_amount(decimal value) => _sut.TestValidate(Valid with { Amount = value }).ShouldHaveValidationErrorFor(x => x.Amount);
    [Fact] public void Rejects_amount_with_excess_currency_precision() => _sut.TestValidate(Valid with { Amount = 1.234m }).ShouldHaveValidationErrorFor(x => x.Amount);
    [Fact] public void Rejects_invalid_payment_method() => _sut.TestValidate(Valid with { PaymentMethod = (PaymentMethod)99 }).ShouldHaveValidationErrorFor(x => x.PaymentMethod);
    [Fact] public void Limits_reference() => _sut.TestValidate(Valid with { Reference = new string('r', PaymentReference.MaxLength + 1) }).ShouldHaveValidationErrorFor(x => x.Reference);
    [Fact] public void Limits_notes() => _sut.TestValidate(Valid with { Notes = new string('n', PaymentNotes.MaxLength + 1) }).ShouldHaveValidationErrorFor(x => x.Notes);
}
