using InvoiceFlow.Customers.Domain;

namespace InvoiceFlow.Customers.Application;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto
        {
            TenantId = customer.TenantId,
            Id = customer.Id,
            Name = customer.Name.Value,
            CustomerCode = customer.CustomerCode.Value,
            CustomerContact = customer.ToContactDto(),
            CustomerTaxDetails = customer.ToTaxDetailsDto(),
            CustomerAddress = customer.ToAddressDto(),
            CurrencyCode = customer.CurrencyCode.Value,
            CustomerCreditPolicy = customer.ToCustomerCreditPolicyDto(),
            IsActive = customer.IsActive,
            CreatedAtUtc = customer.CreatedAtUtc,
            UpdatedAtUtc = customer.UpdatedAtUtc
        };
    }

    private static CustomerContactDto ToContactDto(this Customer customer)
    {
        return new CustomerContactDto
        {
            Email = customer.Email?.Value,
            Phone = customer.Phone?.Value
        };
    }

    private static CustomerAddressDto ToAddressDto(this Customer customer)
    {
        return new CustomerAddressDto
        {
            AddressLine1 = customer.AddressLine1.Value,
            AddressLine2 = customer.AddressLine2?.Value,
            City = customer.City.Value,
            State = customer.State?.Value,
            Country = customer.Country.Value,
            PostalCode = customer.PostalCode.Value
        };
    }

    private static CustomerTaxDetailsDto ToTaxDetailsDto(this Customer customer)
    {
        return new CustomerTaxDetailsDto
        {
            TaxNumber = customer.TaxNumber?.Value,
            RegistrationNumber = customer.RegistrationNumber.Value
        };
    }

    private static CustomerCreditPolicyDto ToCustomerCreditPolicyDto(this Customer customer)
    {
        return new CustomerCreditPolicyDto
        {
            CreditLimit = customer.CreditLimit.Value,
            PaymentTermDays = customer.PaymentTermDays.Value
        };
    }
}
