
namespace InvoiceFlow.Customers.Application;

public sealed record CustomerContact(string? Email, string? Phone);

public sealed record CustomerAddress(
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string Country,
    string PostalCode);

public sealed record CustomerCreditPolicy(decimal CreditLimit, int PaymentTermDays);

public sealed record CustomerTaxDetails(string? TaxNumber, string RegistrationNumber);
