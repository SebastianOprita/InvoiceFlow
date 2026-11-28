using InvoiceFlow.Payments.Application;

namespace InvoiceFlow.Payments.Api;

public static class Mapper
{
    public static CreatePaymentCommand ToCommand(
        this CreatePaymentRequest request,
        string idempotencyKey,
        Guid tenantId,
        Guid createdBy)
    {
        return new CreatePaymentCommand(
            idempotencyKey,
            tenantId,
            createdBy,
            request.CustomerCode,
            request.CurrencyCode,
            request.Amount,
            request.PaymentMethod,
            request.Reference,
            request.Notes);
    }

    public static AddPaymentAllocationCommand ToCommand(
        this AddPaymentAllocationRequest request,
        string idempotencyKey,
        Guid tenantId,
        Guid paymentId,
        Guid allocatedBy)
    {
        return new AddPaymentAllocationCommand(
            idempotencyKey,
            tenantId,
            paymentId,
            request.InvoiceId,
            request.Amount,
            allocatedBy);
    }
}
