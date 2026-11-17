using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Application;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(
        this Invoice invoice,
        InvoicePaymentSummary? paymentSummary = null)
    {
        return new InvoiceDto(
            TenantId: invoice.TenantId,
            Id: invoice.Id,
            Number: invoice.Number?.Value,
            CustomerId: invoice.CustomerId,
            CustomerCode: invoice.CustomerCode.Value,
            CustomerName: invoice.CustomerName.Value,
            CustomerEmail: invoice.CustomerEmail?.Value,
            CustomerPhone: invoice.CustomerPhone?.Value,
            CustomerTaxNumber: invoice.CustomerTaxNumber?.Value,
            CustomerRegistrationNumber: invoice.CustomerRegistrationNumber.Value,
            CustomerAddressLine1: invoice.CustomerAddressLine1.Value,
            CustomerAddressLine2: invoice.CustomerAddressLine2?.Value,
            CustomerCity: invoice.CustomerCity.Value,
            CustomerState: invoice.CustomerState?.Value,
            CustomerCountry: invoice.CustomerCountry.Value,
            CustomerPostalCode: invoice.CustomerPostalCode.Value,
            CurrencyCode: invoice.CurrencyCode.Value,
            PaymentTermDays: invoice.PaymentTermDays.Value,
            IssueDate: invoice.IssueDate,
            DueDate: invoice.DueDate,
            Status: invoice.Status,
            PaymentStatus: paymentSummary?.Status ?? InvoicePaymentStatus.Unpaid,
            DiscountAmount: invoice.DiscountAmount.Amount,
            NetAmount: invoice.NetAmount.Amount,
            TaxAmount: invoice.TaxAmount.Amount,
            GrossAmount: invoice.GrossAmount.Amount,
            PaidAmount: paymentSummary?.PaidAmount.Amount ?? 0m,
            RemainingAmount: paymentSummary?.OutstandingAmount.Amount ?? invoice.GrossAmount.Amount,
            Notes: invoice.Notes?.Value,
            IssuedAtUtc: invoice.IssuedAtUtc,
            CreatedAtUtc: invoice.CreatedAtUtc,
            UpdatedAtUtc: invoice.UpdatedAtUtc,
            Lines: invoice.Lines.Select(line => line.ToDto()).ToArray());
    }

    public static InvoiceLineDto ToDto(this InvoiceLine line)
    {
        return new InvoiceLineDto(
            TenantId: line.TenantId,
            Id: line.Id,
            Position: line.Position,
            ProductId: line.ProductId,
            ProductCode: line.ProductCode?.Value,
            Description: line.Description.Value,
            Quantity: line.Quantity.Value,
            UnitOfMeasure: line.UnitOfMeasure,
            UnitPrice: line.UnitPrice.Amount,
            DiscountRate: line.DiscountRate.Value,
            DiscountAmount: line.DiscountAmount.Amount,
            TaxRate: line.TaxRate.Value,
            TaxAmount: line.TaxAmount.Amount,
            NetAmount: line.NetAmount.Amount,
            GrossAmount: line.GrossAmount.Amount);
    }
}
