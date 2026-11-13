using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Payments.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError InvoicePaymentBalanceInvoiceIdInvalid =
        new(
            "invoicePaymentBalance.invoiceIdInvalid",
            "The invoice identifier is invalid.");

    public static readonly DomainError InvoicePaymentBalanceTotalAmountInvalid =
        new(
            "invoicePaymentBalance.totalAmountInvalid",
            "The invoice total amount must be greater than zero.");

    public static readonly DomainError InvoicePaymentBalanceAllocationAmountInvalid =
        new(
            "invoicePaymentBalance.allocationAmountInvalid",
            "The allocation amount must be greater than zero.");

    public static readonly DomainError InvoicePaymentBalanceClosed =
        new(
            "invoicePaymentBalance.closed",
            "The invoice payment balance is closed.");

    public static readonly DomainError InvoicePaymentBalanceOverAllocation =
        new(
            "invoicePaymentBalance.overAllocation",
            "The allocation amount exceeds the invoice outstanding amount.");

    public static readonly DomainError InvoicePaymentBalanceOverReversal =
        new(
            "invoicePaymentBalance.overReversal",
            "The reversal amount exceeds the allocated amount.");

    public static readonly DomainError InvoicePaymentBalanceHasAllocations =
        new(
            "invoicePaymentBalance.hasAllocations",
            "The invoice payment balance cannot be closed while allocations exist.");

    public static readonly DomainError InvoicePaymentBalanceCurrencyMismatch =
        new(
            "invoicePaymentBalance.currencyMismatch",
            "The allocation currency does not match the invoice currency.");

}
