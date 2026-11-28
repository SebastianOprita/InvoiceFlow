using System.Text.Json.Serialization;

namespace InvoiceFlow.Payments.Api;

public sealed record AddPaymentAllocationRequest(
    [property: JsonRequired] Guid InvoiceId,
    [property: JsonRequired] decimal Amount);
