namespace InvoiceFlow.Customers.Api;

public record UpdateCustomerRequest(
    string Name,
    CustomerContactRequest CustomerContact,
    CustomerAddressRequest CustomerAddress,
    CustomerCreditPolicyRequest CustomerCreditPolicy);
