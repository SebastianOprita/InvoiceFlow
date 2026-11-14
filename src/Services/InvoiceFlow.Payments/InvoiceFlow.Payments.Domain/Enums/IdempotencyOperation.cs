namespace InvoiceFlow.Payments.Domain;

public enum IdempotencyOperation
{
    CreatePayment = 1,
    AddPaymentAllocation = 2
}
