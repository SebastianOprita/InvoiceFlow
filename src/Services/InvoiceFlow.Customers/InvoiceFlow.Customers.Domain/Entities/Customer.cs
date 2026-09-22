using InvoiceFlow.Common.Domain;

namespace InvoiceFlow.Customers.Domain;

public sealed class Customer
{
    public Guid TenantId { get; private set; }
    public Guid Id { get; private set; }
    public CustomerCode CustomerCode { get; private set; }
    public CustomerName Name { get; private set; }
    public CustomerEmail? Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public TaxNumber? TaxNumber { get; private set; }
    public RegistrationNumber RegistrationNumber { get; private set; }
    public AddressLine1 AddressLine1 { get; private set; }
    public AddressLine2? AddressLine2 { get; private set; }
    public City City { get; private set; }
    public State? State { get; private set; }
    public Country Country { get; private set; }
    public PostalCode PostalCode { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; }
    public CreditLimit CreditLimit { get; private set; }
    public PaymentTermDays PaymentTermDays { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

#pragma warning disable CS8618
    private Customer() { } // EF Core
#pragma warning restore CS8618

    private Customer(
        Guid id,
        Guid tenantId,
        CustomerCode customerCode,
        CustomerName name,
        CustomerEmail? email,
        PhoneNumber? phone,
        TaxNumber? taxNumber,
        RegistrationNumber registrationNumber,
        AddressLine1 addressLine1,
        AddressLine2? addressLine2,
        City city,
        State? state,
        Country country,
        PostalCode postalCode,
        CurrencyCode currencyCode,
        CreditLimit creditLimit,
        PaymentTermDays paymentTermDays,
        DateTime createdAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        CustomerCode = customerCode;
        Name = name;
        Email = email;
        Phone = phone;
        TaxNumber = taxNumber;
        RegistrationNumber = registrationNumber;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
        CurrencyCode = currencyCode;
        CreditLimit = creditLimit;
        PaymentTermDays = paymentTermDays;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public static Customer Create(
        Guid id,
        Guid tenantId,
        CustomerCode customerCode,
        CustomerName name,
        CustomerEmail? email,
        PhoneNumber? phone,
        TaxNumber? taxNumber,
        RegistrationNumber registrationNumber,
        AddressLine1 addressLine1,
        AddressLine2? addressLine2,
        City city,
        State? state,
        Country country,
        PostalCode postalCode,
        CurrencyCode currencyCode,
        CreditLimit creditLimit,
        PaymentTermDays paymentTermDays,
        DateTime createdAtUtc)
    {
        if (tenantId == Guid.Empty)
            throw new DomainException(DomainErrors.TenantIdRequired);

        if (id == Guid.Empty)
            throw new DomainException(DomainErrors.IdRequired);

        if (createdAtUtc == default)
            throw new DomainException(DomainErrors.CreatedAtUtcRequired);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.CreatedAtUtcNotUtc);

        return new Customer(
            id,
            tenantId,
            customerCode,
            name,
            email,
            phone,
            taxNumber,
            registrationNumber,
            addressLine1,
            addressLine2,
            city,
            state,
            country,
            postalCode,
            currencyCode,
            creditLimit,
            paymentTermDays,
            createdAtUtc);
    }

    public void UpdateName(CustomerName name, DateTime updatedAtUtc)
    {
        if (Name == name)
            return;

        Name = name;

        MarkAsUpdated(updatedAtUtc);
    }

    public void UpdateContact(
        CustomerEmail? email,
        PhoneNumber? phone, DateTime updatedAtUtc)
    {
        if (Email == email && Phone == phone)
            return;

        Email = email;
        Phone = phone;

        MarkAsUpdated(updatedAtUtc);
    }

    public void UpdateAddress(
        AddressLine1 addressLine1,
        AddressLine2? addressLine2,
        City city,
        State? state,
        Country country,
        PostalCode postalCode, DateTime updatedAtUtc)
    {
        if (AddressLine1 == addressLine1 &&
            AddressLine2 == addressLine2 &&
            City == city &&
            State == state &&
            Country == country &&
            PostalCode == postalCode)
            return;

        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;

        MarkAsUpdated(updatedAtUtc);
    }

    public void UpdateCreditPolicy(
        CreditLimit creditLimit,
        PaymentTermDays paymentTermDays, DateTime updatedAtUtc)
    {
        if (CreditLimit == creditLimit && PaymentTermDays == paymentTermDays)
            return;

        CreditLimit = creditLimit;
        PaymentTermDays = paymentTermDays;

        MarkAsUpdated(updatedAtUtc);
    }

    public void UpdateTaxDetails(
        TaxNumber? taxNumber,
        RegistrationNumber registrationNumber, DateTime updatedAtUtc)
    {
        if (TaxNumber == taxNumber && RegistrationNumber == registrationNumber)
            return;

        TaxNumber = taxNumber;
        RegistrationNumber = registrationNumber;

        MarkAsUpdated(updatedAtUtc);
    }

    public void ChangeCurrency(CurrencyCode currencyCode, DateTime updatedAtUtc)
    {
        if (CurrencyCode == currencyCode)
            return;

        CurrencyCode = currencyCode;

        MarkAsUpdated(updatedAtUtc);
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        if (!IsActive)
            return;

        IsActive = false;

        MarkAsUpdated(updatedAtUtc);
    }

    public void Activate(DateTime updatedAtUtc)
    {
        if (IsActive)
            return;

        IsActive = true;

        MarkAsUpdated(updatedAtUtc);
    }

    private void MarkAsUpdated(DateTime updatedAtUtc)
    {
        if (updatedAtUtc == default)
            throw new DomainException(DomainErrors.UpdatedAtUtcRequired);

        if (updatedAtUtc.Kind != DateTimeKind.Utc)
            throw new DomainException(DomainErrors.UpdatedAtUtcNotUtc);

        if (updatedAtUtc < CreatedAtUtc)
            throw new DomainException(DomainErrors.UpdatedAtUtcInvalid);

        UpdatedAtUtc = updatedAtUtc;
    }
}
