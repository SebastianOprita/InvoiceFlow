using System.Text.Json.Serialization;

namespace InvoiceFlow.Customers.Api;

public record CreateCustomerRequest(
    [property: JsonRequired] string Name,
    [property: JsonRequired] string CustomerCode,
    [property: JsonRequired] CustomerContactRequest CustomerContact,
    [property: JsonRequired] CustomerTaxDetailsRequest CustomerTaxDetails,
    [property: JsonRequired] CustomerAddressRequest CustomerAddress,
    [property: JsonRequired] string CurrencyCode,
    [property: JsonRequired] CustomerCreditPolicyRequest CustomerCreditPolicy);
