using System.Text.Json.Serialization;

namespace InvoiceFlow.Invoices.Api;

public record CreateInvoiceRequest(
    [property: JsonRequired] string CustomerCode,
    string InvoiceNotes);
