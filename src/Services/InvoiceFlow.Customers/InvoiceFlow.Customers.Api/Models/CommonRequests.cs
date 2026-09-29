namespace InvoiceFlow.Customers.Api;

public record CustomerContactRequest(
    string? Email,
    string? Phone);

public record CustomerAddressRequest(
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string Country,
    string PostalCode);

public record CustomerCreditPolicyRequest(
    decimal CreditLimit,
    int PaymentTermDays);

public record CustomerTaxDetailsRequest(
    string? TaxNumber,
    string RegistrationNumber);
