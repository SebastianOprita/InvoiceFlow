using System.Text.Json.Serialization;

namespace InvoiceFlow.Customers.Api;

public record CustomerContactRequest(
    [property: JsonRequired] string? Email,
    [property: JsonRequired] string? Phone);

public record CustomerAddressRequest(
    [property: JsonRequired] string AddressLine1,
    string? AddressLine2,
    [property: JsonRequired] string City,
    string? State,
    [property: JsonRequired] string Country,
    [property: JsonRequired] string PostalCode);

public record CustomerCreditPolicyRequest(
    [property: JsonRequired] decimal CreditLimit,
    [property: JsonRequired] int PaymentTermDays);

public record CustomerTaxDetailsRequest(
    string? TaxNumber,
    [property: JsonRequired] string RegistrationNumber);
