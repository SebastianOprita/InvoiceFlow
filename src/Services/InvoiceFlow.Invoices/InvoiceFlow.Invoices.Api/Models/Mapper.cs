using InvoiceFlow.Invoices.Application;

namespace InvoiceFlow.Invoices.Api;

public static class Mapper
{
    public static CreateInvoiceCommand ToCommand(this CreateInvoiceRequest request, string IdempotencyKey, Guid tenantId)
    {
        return new CreateInvoiceCommand(
            IdempotencyKey,
            tenantId,
            request.CustomerCode,
            request.InvoiceNotes);
    }

    public static AddInvoiceLineCommand ToCommand(this AddInvoiceLineRequest request, string IdempotencyKey, Guid tenantId, Guid invoiceId)
    {
        return new AddInvoiceLineCommand(
            IdempotencyKey,
            tenantId,
            invoiceId,
            request.ProductCode,
            request.Quantity,
            request.DiscountRate);
    }
}
