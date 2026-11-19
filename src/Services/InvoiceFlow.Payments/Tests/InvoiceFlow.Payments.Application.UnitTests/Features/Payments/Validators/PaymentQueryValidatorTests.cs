using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class PaymentQueryValidatorTests
{
    [Fact]
    public void Get_payment_by_id_requires_tenant_and_payment()
    {
        var result = new GetPaymentByIdQueryValidator().TestValidate(new GetPaymentByIdQuery(Guid.Empty, Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
        result.ShouldHaveValidationErrorFor(x => x.PaymentId);
    }
    [Fact] public void Get_payments_requires_tenant() => new GetPaymentsQueryValidator().TestValidate(new GetPaymentsQuery(Guid.Empty)).ShouldHaveValidationErrorFor(x => x.TenantId);
    [Fact] public void Get_payments_accepts_tenant() => new GetPaymentsQueryValidator().TestValidate(new GetPaymentsQuery(Guid.CreateVersion7())).ShouldNotHaveAnyValidationErrors();
}
