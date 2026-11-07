using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Application;

public static class PaymentMapper
{
    public static PaymentDto ToDto(this Payment payment)
    {
        return new PaymentDto(
            TenantId: payment.TenantId,
            Id: payment.Id,
            Number: payment.Number.Value,
            CustomerId: payment.CustomerId,
            CurrencyCode: payment.CurrencyCode.Value,
            Amount: payment.Amount.Amount,
            AllocatedAmount: payment.AllocatedAmount.Amount,
            UnallocatedAmount: payment.UnallocatedAmount.Amount,
            Method: payment.Method,
            Reference: payment.Reference?.Value,
            PaymentDate: payment.PaymentDate,
            Status: payment.Status,
            Notes: payment.Notes?.Value,
            PostedAtUtc: payment.PostedAtUtc,
            CancelledAtUtc: payment.CancelledAtUtc,
            CreatedAtUtc: payment.CreatedAtUtc,
            UpdatedAtUtc: payment.UpdatedAtUtc,
            Allocations: payment.Allocations.Select(allocation => allocation.ToDto()).ToArray());
    }

    public static PaymentAllocationDto ToDto(this PaymentAllocation allocation)
    {
        return new PaymentAllocationDto(
            TenantId: allocation.TenantId,
            Id: allocation.Id,
            PaymentId: allocation.PaymentId,
            InvoiceId: allocation.InvoiceId,
            InvoiceNumber: allocation.InvoiceNumber.Value,
            Amount: allocation.Amount.Amount,
            CurrencyCode: allocation.Amount.CurrencyCode.Value,
            CreatedAtUtc: allocation.CreatedAtUtc);
    }
}
