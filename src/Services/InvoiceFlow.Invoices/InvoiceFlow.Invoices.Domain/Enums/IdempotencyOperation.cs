namespace InvoiceFlow.Invoices.Domain;

public enum IdempotencyOperation
{
    CreateInvoice = 1,
    AddInvoiceLine = 2
}
