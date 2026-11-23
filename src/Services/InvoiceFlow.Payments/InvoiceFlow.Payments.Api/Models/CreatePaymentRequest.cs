using System.Text.Json.Serialization;
using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Api;

public sealed record CreatePaymentRequest(
    [property: JsonRequired] string CustomerCode,
    [property: JsonRequired] string CurrencyCode,
    [property: JsonRequired] decimal Amount,
    [property: JsonRequired] PaymentMethod PaymentMethod,
    [property: JsonRequired] string Reference,
    string Notes);
