using FluentAssertions;
using FluentValidation.TestHelper;
using InvoiceFlow.Payments.Application;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class AddPaymentAllocationCommandValidatorTests
{
    private readonly AddPaymentAllocationCommandValidator _sut = new();

    private static readonly AddPaymentAllocationCommand Valid = new(
        "key-1",
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        Guid.CreateVersion7(),
        20m,
        Guid.CreateVersion7());

    [Fact]
    public void Valid_command_has_no_errors() =>
        _sut.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Requires_idempotency_key(string? value) =>
        _sut.TestValidate(Valid with { IdempotencyKey = value! })
            .ShouldHaveValidationErrorFor(x => x.IdempotencyKey);

    [Fact]
    public void Limits_idempotency_key() =>
        _sut.TestValidate(Valid with { IdempotencyKey = new string('k', 129) })
            .ShouldHaveValidationErrorFor(x => x.IdempotencyKey);

    [Fact]
    public void Requires_tenant() =>
        _sut.TestValidate(Valid with { TenantId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.TenantId);

    [Fact]
    public void Requires_payment() =>
        _sut.TestValidate(Valid with { PaymentId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.PaymentId);

    [Fact]
    public void Requires_invoice() =>
        _sut.TestValidate(Valid with { InvoiceId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.InvoiceId);

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Requires_positive_amount(decimal amount) =>
        _sut.TestValidate(Valid with { Amount = amount })
            .ShouldHaveValidationErrorFor(x => x.Amount);

    [Fact]
    public void Requires_allocated_by() =>
        _sut.TestValidate(Valid with { AllocatedBy = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.AllocatedBy);
}
