using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public static partial class DomainErrors
{
    public static readonly DomainError CustomerInactive = new($"customer.inactive", "Customer is inactive.");
    public static readonly DomainError CurrencyCodeRequired = new("currencyCode.required", "Currency code is required.");
    public static readonly DomainError CurrencyCodeInvalid = new("currencyCode.invalid", "Currency code is invalid. Expected a valid ISO 4217 currency code.");
    public static readonly DomainError CustomerCodeRequired = new("customerCode.required", "Customer code is required.");
    public static readonly DomainError CustomerCodeTooLong = new("customerCode.tooLong", $"Customer code is too long. Maximum length is {CustomerCode.MaxLength} characters.");
    public static readonly DomainError CustomerEmailRequired = new("customerEmail.required", "Email is required.");
    public static readonly DomainError CustomerEmailTooLong = new("customerEmail.tooLong", $"Email is too long. Maximum length is {CustomerEmail.MaxLength} characters.");
    public static readonly DomainError CustomerEmailInvalid = new("customerEmail.invalid", "Email is invalid.");
    public static readonly DomainError CustomerTaxNumberRequired = new("customerTaxNumber.required", "Tax number is required.");
    public static readonly DomainError CustomerTaxNumberTooLong = new("customerTaxNumber.tooLong", $"Tax number is too long. Maximum length is {TaxNumber.MaxLength} characters.");
    public static readonly DomainError CustomerRegistrationNumberRequired = new("customerRegistrationNumber.required", "Registration number is required.");
    public static readonly DomainError CustomerRegistrationNumberTooLong = new("customerRegistrationNumber.tooLong", $"Registration number is too long. Maximum length is {RegistrationNumber.MaxLength} characters.");
    public static readonly DomainError CustomerPhoneRequired = new("customerPhone.required", "Phone is required.");
    public static readonly DomainError CustomerPhoneTooLong = new("customerPhone.tooLong", $"Phone is too long. Maximum length is {PhoneNumber.MaxLength} characters.");
    public static readonly DomainError CustomerNameRequired = new("customerName.required", "Customer name is required.");
    public static readonly DomainError CustomerNameTooLong = new("customerName.tooLong", $"Customer name is too long. Maximum length is {CustomerName.MaxLength} characters.");
    public static readonly DomainError CustomerAddressLine1Required = new("customerAddressLine1.required", "Address line 1 is required.");
    public static readonly DomainError CustomerAddressLine1TooLong = new("customerAddressLine1.tooLong", $"Address line 1 is too long. Maximum length is {AddressLine1.MaxLength} characters.");
    public static readonly DomainError CustomerAddressLine2Required = new("customerAddressLine2.required", "Address line 2 is required.");
    public static readonly DomainError CustomerAddressLine2TooLong = new("customerAddressLine2.tooLong", $"Address line 2 is too long. Maximum length is {AddressLine2.MaxLength} characters.");
    public static readonly DomainError CustomerCityRequired = new("customerCity.required", "City is required.");
    public static readonly DomainError CustomerCityTooLong = new("customerCity.tooLong", $"City is too long. Maximum length is {City.MaxLength} characters.");
    public static readonly DomainError CustomerStateRequired = new("customerState.required", "State is required.");
    public static readonly DomainError CustomerStateTooLong = new("customerState.tooLong", $"State is too long. Maximum length is {State.MaxLength} characters.");
    public static readonly DomainError CustomerCountryRequired = new("customerCountry.required", "Country is required.");
    public static readonly DomainError CustomerCountryTooLong = new("customerCountry.tooLong", $"Country is too long. Maximum length is {Country.MaxLength} characters.");
    public static readonly DomainError CustomerPaymentTermDaysInvalid = new("paymentTermDays.invalid", "Payment term days cannot be negative.");
    public static readonly DomainError CustomerPostalCodeRequired = new("customerPostalCode.required", "Postal code is required.");
    public static readonly DomainError CustomerPostalCodeTooLong = new("customerPostalCode.tooLong", $"Postal code is too long. Maximum length is {PostalCode.MaxLength} characters.");
    public static readonly DomainError CreditLimitInvalid = new("creditLimit.invalid", "Credit Limit cannot be negative.");
}
