using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public sealed record InvoiceCustomerSnapshot
{
    public Guid CustomerTenantId { get; }
    public Guid CustomerId { get; }
    public bool IsActive { get; }
    public CustomerCode CustomerCode { get; }
    public CustomerName CustomerName { get; }
    public CustomerEmail? CustomerEmail { get; }
    public PhoneNumber? CustomerPhone { get; }
    public TaxNumber? CustomerTaxNumber { get; }
    public RegistrationNumber CustomerRegistrationNumber { get; }
    public AddressLine1 CustomerAddressLine1 { get; }
    public AddressLine2? CustomerAddressLine2 { get; }
    public City CustomerCity { get; }
    public State? CustomerState { get; }
    public Country CustomerCountry { get; }
    public PostalCode CustomerPostalCode { get; }
    public CurrencyCode CurrencyCode { get; }
    public CreditLimit CreditLimit { get; }
    public PaymentTermDays PaymentTermDays { get; }

    private InvoiceCustomerSnapshot(
        Guid customerTenantId,
        Guid customerId,
        bool isActive,
        CustomerCode customerCode,
        CustomerName customerName,
        CustomerEmail? customerEmail,
        PhoneNumber? customerPhone,
        TaxNumber? customerTaxNumber,
        RegistrationNumber customerRegistrationNumber,
        AddressLine1 customerAddressLine1,
        AddressLine2? customerAddressLine2,
        City customerCity,
        State? customerState,
        Country customerCountry,
        PostalCode customerPostalCode,
        CurrencyCode currencyCode,
        CreditLimit creditLimit,
        PaymentTermDays paymentTermDays)
    {
        CustomerTenantId = customerTenantId;
        CustomerId = customerId;
        IsActive = isActive;
        CustomerCode = customerCode;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        CustomerTaxNumber = customerTaxNumber;
        CustomerRegistrationNumber = customerRegistrationNumber;
        CustomerAddressLine1 = customerAddressLine1;
        CustomerAddressLine2 = customerAddressLine2;
        CustomerCity = customerCity;
        CustomerState = customerState;
        CustomerCountry = customerCountry;
        CustomerPostalCode = customerPostalCode;
        CurrencyCode = currencyCode;
        CreditLimit = creditLimit;
        PaymentTermDays = paymentTermDays;
    }

    public static InvoiceCustomerSnapshot Create(
        Guid customerTenantId,
        Guid customerId,
        bool isActive,
        CustomerCode customerCode,
        CustomerName customerName,
        CustomerEmail? customerEmail,
        PhoneNumber? customerPhone,
        TaxNumber? customerTaxNumber,
        RegistrationNumber customerRegistrationNumber,
        AddressLine1 customerAddressLine1,
        AddressLine2? customerAddressLine2,
        City customerCity,
        State? customerState,
        Country customerCountry,
        PostalCode customerPostalCode,
        CurrencyCode currencyCode,
        CreditLimit creditLimit,
        PaymentTermDays paymentTermDays)
    {
        if (customerTenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (customerId == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        return new InvoiceCustomerSnapshot(
            customerTenantId,
            customerId,
            isActive,
            customerCode,
            customerName,
            customerEmail,
            customerPhone,
            customerTaxNumber,
            customerRegistrationNumber,
            customerAddressLine1,
            customerAddressLine2,
            customerCity,
            customerState,
            customerCountry,
            customerPostalCode,
            currencyCode,
            creditLimit,
            paymentTermDays);
    }
}
