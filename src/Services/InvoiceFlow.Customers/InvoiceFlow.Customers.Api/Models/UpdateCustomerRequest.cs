using System.Text.Json.Serialization;

namespace InvoiceFlow.Customers.Api;

public record UpdateCustomerRequest(
    [property: JsonRequired] string Name,
    [property: JsonRequired] CustomerContactRequest CustomerContact,
    [property: JsonRequired] CustomerAddressRequest CustomerAddress,
    [property: JsonRequired] CustomerCreditPolicyRequest CustomerCreditPolicy);
