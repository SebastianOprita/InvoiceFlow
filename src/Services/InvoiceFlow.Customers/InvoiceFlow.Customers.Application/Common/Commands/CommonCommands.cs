
namespace InvoiceFlow.Customers.Application;

public record CustomerContact(string? Email, string? Phone);

public record CustomerAddress(
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? State,
    string Country,
    string PostalCode);

public record CustomerCreditPolicy(decimal CreditLimit, int PaymentTermDays);

public record CustomerTaxDetails(string? TaxNumber, string RegistrationNumber);
