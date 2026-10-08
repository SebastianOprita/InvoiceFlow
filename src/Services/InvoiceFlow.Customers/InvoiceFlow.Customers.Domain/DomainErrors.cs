using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Customers.Domain;

public static class DomainErrors
{
    public static readonly DomainError TenantIdRequired = new("tenantId.required", "Tenant Id is required.");
    public static readonly DomainError IdRequired = new("id.required", "Id is required.");
    public static readonly DomainError CreatedAtUtcRequired = new("createdAtUtc.required", "Created datetime is required.");
    public static readonly DomainError UpdatedAtUtcRequired = new("updatedAtUtc.required", "Updated datetime is required.");
    public static readonly DomainError CreatedAtUtcNotUtc = new("createdAtUtc.notUtc", "Created datetime must be UTC.");
    public static readonly DomainError UpdatedAtUtcNotUtc = new("updatedAtUtc.notUtc", "Updated datetime must be UTC.");
    public static readonly DomainError UpdatedAtUtcInvalid = new("updatedAtUtc.invalid", "Updated datetime cannot be earlier than Created datetime.");
    public static readonly DomainError AddressLine1Required = new("addressLine1.required", "Address Line 1 is required.");
    public static readonly DomainError AddressLine1TooLong = new("addressLine1.tooLong", $"Address Line 1 is too long. Maximum length is {AddressLine1.MaxLength} characters.");
    public static readonly DomainError AddressLine2Required = new("addressLine2.required", "Address Line 2 is required.");
    public static readonly DomainError AddressLine2TooLong = new("addressLine2.tooLong", $"Address Line 2 is too long. Maximum length is {AddressLine2.MaxLength} characters.");
    public static readonly DomainError CityRequired = new("city.required", "City is required.");
    public static readonly DomainError CityTooLong = new("city.tooLong", $"City is too long. Maximum length is {City.MaxLength} characters.");
    public static readonly DomainError CountryRequired = new("country.required", "Country is required.");
    public static readonly DomainError CountryTooLong = new("country.tooLong", $"Country is too long. Maximum length is {Country.MaxLength} characters.");
    public static readonly DomainError CreditLimitInvalid = new("creditLimit.invalid", "Credit Limit cannot be negative.");
    public static readonly DomainError CurrencyCodeRequired = new("currencyCode.required", "Currency Code is required.");
    public static readonly DomainError CurrencyCodeInvalid = new("currencyCode.invalid", $"Currency Code must be a {CurrencyCode.MaxLength} letter code.");
    public static readonly DomainError CustomerCodeRequired = new("customerCode.required", "Customer Code is required.");
    public static readonly DomainError CustomerCodeTooLong = new("customerCode.tooLong", $"Customer Code is too long. Maximum length is {CustomerCode.MaxLength} characters.");
    public static readonly DomainError EmailRequired = new("email.required", "Email is required.");
    public static readonly DomainError EmailTooLong = new("email.tooLong", $"Email is too long. Maximum length is {CustomerEmail.MaxLength} characters.");
    public static readonly DomainError EmailInvalid = new("email.invalid", "Email is invalid.");
    public static readonly DomainError CustomerNameRequired = new("name.required", "Name is required.");
    public static readonly DomainError CustomerNameTooLong = new("name.tooLong", $"Name is too long. Maximum length is {CustomerName.MaxLength} characters.");
    public static readonly DomainError PaymentTermDaysInvalid = new("paymentTermDays.invalid", "Payment Term days cannot be negative.");
    public static readonly DomainError PhoneNumberRequired = new("phone.required", "Phone is required.");
    public static readonly DomainError PhoneNumberTooLong = new("phone.tooLong", $"Phone is too long. Maximum length is {PhoneNumber.MaxLength} characters.");
    public static readonly DomainError PostalCodeRequired = new("postalCode.required", "Postal Code is required.");
    public static readonly DomainError PostalCodeTooLong = new("postalCode.tooLong", $"Postal Code is too long. Maximum length is {PostalCode.MaxLength} characters.");
    public static readonly DomainError RegistrationNumberRequired = new("registrationNumber.required", "Registration Number is required.");
    public static readonly DomainError RegistrationNumberTooLong = new("registrationNumber.tooLong", $"Registration Number is too long. Maximum length is {RegistrationNumber.MaxLength} characters.");
    public static readonly DomainError StateRequired = new("state.required", "State is required.");
    public static readonly DomainError StateTooLong = new("state.tooLong", $"State is too long. Maximum length is {State.MaxLength} characters.");
    public static readonly DomainError TaxNumberRequired = new("taxNumber.required", "Tax Number is required.");
    public static readonly DomainError TaxNumberTooLong = new("taxNumber.tooLong", $"Tax Number is too long. Maximum length is {TaxNumber.MaxLength} characters.");
}
