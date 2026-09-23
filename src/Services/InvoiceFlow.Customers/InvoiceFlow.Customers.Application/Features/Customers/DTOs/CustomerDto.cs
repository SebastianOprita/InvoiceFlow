namespace InvoiceFlow.Customers.Application;

public record CustomerDto
{
    public required Guid TenantId { get; set; }
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string CustomerCode { get; set; }
    public required CustomerContactDto CustomerContact { get; set; }
    public required CustomerTaxDetailsDto CustomerTaxDetails { get; set; }
    public required CustomerAddressDto CustomerAddress { get; set; }
    public required string CurrencyCode { get; set; }
    public required CustomerCreditPolicyDto CustomerCreditPolicy { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public record CustomerContactDto
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public record CustomerAddressDto
{
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? State { get; set; }
    public required string Country { get; set; }
    public required string PostalCode { get; set; }
}

public record CustomerCreditPolicyDto
{
    public required decimal CreditLimit { get; set; }
    public required int PaymentTermDays { get; set; }
}

public record CustomerTaxDetailsDto
{
    public string? TaxNumber { get; set; }
    public required string RegistrationNumber { get; set; }
}
