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
}
