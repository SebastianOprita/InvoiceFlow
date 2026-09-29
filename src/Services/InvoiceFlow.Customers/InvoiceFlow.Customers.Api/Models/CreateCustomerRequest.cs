namespace InvoiceFlow.Customers.Api;

public record CreateCustomerRequest(
    string Name,
    string CustomerCode,
    CustomerContactRequest CustomerContact,
    CustomerTaxDetailsRequest CustomerTaxDetails,
    CustomerAddressRequest CustomerAddress,
    string CurrencyCode,
    CustomerCreditPolicyRequest CustomerCreditPolicy);
