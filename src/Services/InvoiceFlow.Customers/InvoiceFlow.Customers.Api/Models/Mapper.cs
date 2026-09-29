using InvoiceFlow.Customers.Application;

namespace InvoiceFlow.Customers.Api;

public static class Mapper
{
    public static CreateCustomerCommand ToCommand(this CreateCustomerRequest request, Guid tenantId)
    {
        return new CreateCustomerCommand(
            tenantId,
            request.CustomerCode,
            request.Name,
            request.CustomerContact.ToCustomerContact(),
            request.CustomerTaxDetails.ToCustomerTaxDetails(),
            request.CustomerAddress.ToCustomerAddress(),
            request.CurrencyCode,
            request.CustomerCreditPolicy.ToCustomerCreditPolicy());
    }

    public static UpdateCustomerCommand ToCommand(this UpdateCustomerRequest request, Guid tenantId, Guid customerId)
    {
        return new UpdateCustomerCommand(
            tenantId,
            customerId,
            request.Name,
            request.CustomerContact.ToCustomerContact(),
            request.CustomerAddress.ToCustomerAddress(),
            request.CustomerCreditPolicy.ToCustomerCreditPolicy());
    }

    private static CustomerContact ToCustomerContact(this CustomerContactRequest request)
    {
        return new CustomerContact(request.Email, request.Phone);
    }

    private static CustomerAddress ToCustomerAddress(this CustomerAddressRequest request)
    {
        return new CustomerAddress
        (
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.Country,
            request.PostalCode
        );
    }

    private static CustomerCreditPolicy ToCustomerCreditPolicy(this CustomerCreditPolicyRequest request)
    {
        return new CustomerCreditPolicy(request.CreditLimit, request.PaymentTermDays);
    }

    private static CustomerTaxDetails ToCustomerTaxDetails(this CustomerTaxDetailsRequest request)
    {
        return new CustomerTaxDetails(request.TaxNumber, request.RegistrationNumber);
    }
}
