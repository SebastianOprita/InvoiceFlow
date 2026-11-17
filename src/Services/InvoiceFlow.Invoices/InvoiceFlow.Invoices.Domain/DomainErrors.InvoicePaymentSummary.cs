using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError PaymentBalanceVersionInvalid = new(
        "InvoicePaymentSummary.PaymentBalanceVersionInvalid",
        "The payment balance version cannot be negative.");

    public static readonly DomainError PaymentSummaryAmountInvalid = new(
        "InvoicePaymentSummary.AmountInvalid",
        "The paid amount and outstanding amount cannot be negative.");

    public static readonly DomainError PaymentSummaryTimestampInvalid = new(
        "InvoicePaymentSummary.TimestampInvalid",
        "The payment summary source timestamp cannot be earlier than the previously recorded timestamp.");
}
