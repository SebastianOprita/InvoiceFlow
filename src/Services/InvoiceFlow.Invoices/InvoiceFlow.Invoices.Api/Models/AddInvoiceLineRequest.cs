using System.Text.Json.Serialization;

namespace InvoiceFlow.Invoices.Api;

public record AddInvoiceLineRequest(
    [property: JsonRequired] string ProductCode,
    [property: JsonRequired] decimal Quantity,
    decimal? DiscountRate);
