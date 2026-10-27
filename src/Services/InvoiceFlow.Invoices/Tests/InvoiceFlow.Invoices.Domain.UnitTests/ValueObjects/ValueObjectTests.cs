using FluentAssertions;
using InvoiceFlow.BuildingBlocks.Domain;
using static InvoiceFlow.Invoices.Domain.UnitTests.ValueObjects.ValueObjectTestAssertions;
using Xunit;

namespace InvoiceFlow.Invoices.Domain.UnitTests.ValueObjects;

public sealed class AddressLine1Tests
{
    [Fact] public void Create_TrimsValue() => AddressLine1.Create(" Street ").Value.Should().Be("Street");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => AddressLine1.Create(" "), DomainErrors.CustomerAddressLine1Required);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => AddressLine1.Create(new string('x', AddressLine1.MaxLength + 1)), DomainErrors.CustomerAddressLine1TooLong);
}

public sealed class AddressLine2Tests
{
    [Fact] public void Create_TrimsValueAndSupportsOptional() { AddressLine2.Create(" Building ").Value.Should().Be("Building"); AddressLine2.CreateOptional(" ").Should().BeNull(); }
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => AddressLine2.Create(null!), DomainErrors.CustomerAddressLine2Required);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => AddressLine2.Create(new string('x', AddressLine2.MaxLength + 1)), DomainErrors.CustomerAddressLine2TooLong);
}

public sealed class CityTests
{
    [Fact] public void Create_TrimsValue() => City.Create(" Bucharest ").Value.Should().Be("Bucharest");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => City.Create(" "), DomainErrors.CustomerCityRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => City.Create(new string('x', City.MaxLength + 1)), DomainErrors.CustomerCityTooLong);
}

public sealed class CountryTests
{
    [Fact] public void Create_TrimsValue() => Country.Create(" Romania ").Value.Should().Be("Romania");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => Country.Create(null!), DomainErrors.CustomerCountryRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => Country.Create(new string('x', Country.MaxLength + 1)), DomainErrors.CustomerCountryTooLong);
}

public sealed class CreditLimitTests
{
    [Fact] public void Create_AllowsZeroAndPositiveValues() { CreditLimit.Create(0).Value.Should().Be(0); CreditLimit.Create(10.25m).Value.Should().Be(10.25m); }
    [Fact] public void Create_RejectsNegativeValue() => AssertError(() => CreditLimit.Create(-0.01m), DomainErrors.CreditLimitInvalid);
}

public sealed class CurrencyCodeTests
{
    [Fact] public void Create_NormalizesValidIsoCode() { var code = CurrencyCode.Create(" ron "); code.Value.Should().Be("RON"); code.DecimalPlaces.Should().Be(2); code.ToString().Should().Be("RON"); }
    [Fact] public void Create_RejectsMissingCode() => AssertError(() => CurrencyCode.Create(" "), DomainErrors.CurrencyCodeRequired);
    [Fact] public void Create_RejectsUnknownCode() => AssertError(() => CurrencyCode.Create("XYZ"), DomainErrors.CurrencyCodeInvalid);
}

public sealed class CustomerCodeTests
{
    [Fact] public void Create_TrimsValue() => CustomerCode.Create(" CUST-1 ").Value.Should().Be("CUST-1");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => CustomerCode.Create(" "), DomainErrors.CustomerCodeRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => CustomerCode.Create(new string('x', CustomerCode.MaxLength + 1)), DomainErrors.CustomerCodeTooLong);
}

public sealed class CustomerEmailTests
{
    [Fact] public void Create_NormalizesEmailAndSupportsOptional() { CustomerEmail.Create(" USER@EXAMPLE.COM ").Value.Should().Be("user@example.com"); CustomerEmail.CreateOptional(null).Should().BeNull(); }
    [Fact] public void Create_RejectsMissingEmail() => AssertError(() => CustomerEmail.Create(" "), DomainErrors.CustomerEmailRequired);
    [Fact] public void Create_RejectsMalformedEmail() => AssertError(() => CustomerEmail.Create("not-an-email"), DomainErrors.CustomerEmailInvalid);
    [Fact] public void Create_RejectsOverlongEmail() => AssertError(() => CustomerEmail.Create(new string('a', CustomerEmail.MaxLength - 3) + "@x.io"), DomainErrors.CustomerEmailTooLong);
}

public sealed class CustomerNameTests
{
    [Fact] public void Create_TrimsValue() => CustomerName.Create(" Acme ").Value.Should().Be("Acme");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => CustomerName.Create(null!), DomainErrors.CustomerNameRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => CustomerName.Create(new string('x', CustomerName.MaxLength + 1)), DomainErrors.CustomerNameTooLong);
}

public sealed class IdempotencyKeyTests
{
    [Fact] public void Create_TrimsValue() => IdempotencyKey.Create(" key ").Value.Should().Be("key");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => IdempotencyKey.Create(" "), DomainErrors.IdempotencyKeyRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => IdempotencyKey.Create(new string('k', IdempotencyKey.MaxLength + 1)), DomainErrors.IdempotencyKeyTooLong);
}

public sealed class InvoiceLineDescriptionTests
{
    [Fact] public void Create_TrimsValueAndFormatsToString() { var value = InvoiceLineDescription.Create(" Consulting "); value.Value.Should().Be("Consulting"); value.ToString().Should().Be("Consulting"); }
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => InvoiceLineDescription.Create(" "), DomainErrors.InvoiceLineDescriptionRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => InvoiceLineDescription.Create(new string('x', InvoiceLineDescription.MaxLength + 1)), DomainErrors.InvoiceLineDescriptionTooLong);
}

public sealed class InvoiceNotesTests
{
    [Fact] public void Create_TrimsValueAndSupportsOptional() { InvoiceNotes.Create(" Notes ").Value.Should().Be("Notes"); InvoiceNotes.CreateOptional(" ").Should().BeNull(); InvoiceNotes.CreateOptional(" optional ")!.Value.Should().Be("optional"); }
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => InvoiceNotes.Create(null!), DomainErrors.InvoiceNotesRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => InvoiceNotes.Create(new string('x', InvoiceNotes.MaxLength + 1)), DomainErrors.InvoiceNotesTooLong);
}

public sealed class InvoiceNumberTests
{
    [Fact] public void Create_TrimsAndUppercasesValue() => InvoiceNumber.Create(" inv-1 ").Value.Should().Be("INV-1");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => InvoiceNumber.Create(" "), DomainErrors.InvoiceNumberRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => InvoiceNumber.Create(new string('x', InvoiceNumber.MaxLength + 1)), DomainErrors.InvoiceNumberTooLong);
    [Theory] [InlineData(null)] [InlineData("")] [InlineData("   ")] public void CreateOptional_ReturnsNullForMissingValue(string? value) => InvoiceNumber.CreateOptional(value).Should().BeNull();
    [Fact] public void CreateOptional_CreatesNormalizedNumber() => InvoiceNumber.CreateOptional(" inv-1 ")!.Value.Should().Be("INV-1");
}

public sealed class InvoiceNumberPrefixTests
{
    [Fact] public void Create_TrimsValue() => InvoiceNumberPrefix.Create(" INV ").Value.Should().Be("INV");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => InvoiceNumberPrefix.Create(" "), DomainErrors.PrefixRequired);
}

public sealed class InvoiceYearTests
{
    [Theory] [InlineData(2000)] [InlineData(2100)] public void Create_AllowsBoundaryYears(int year) => InvoiceYear.Create(year).Value.Should().Be(year);
    [Theory] [InlineData(1999)] [InlineData(2101)] public void Create_RejectsYearsOutsideRange(int year) => AssertError(() => InvoiceYear.Create(year), DomainErrors.YearInvalid);
}

public sealed class MoneyTests
{
    [Fact] public void Create_AcceptsValidAmountAndFormats() { var money = Money.Create(12.34m, CurrencyCode.Create("RON")); money.Amount.Should().Be(12.34m); money.ToString().Should().Be("12.34 RON"); }
    [Fact] public void Zero_CreatesZeroForCurrency() => Money.Zero(CurrencyCode.Create("JPY")).ToString().Should().Be("0 JPY");
    [Fact] public void Create_RejectsNegativeAmount() => AssertError(() => Money.Create(-1m, CurrencyCode.Create("RON")), DomainErrors.MoneyInvalid);
    [Fact] public void Create_RejectsInvalidCurrencyPrecision() => AssertError(() => Money.Create(1.234m, CurrencyCode.Create("RON")), DomainErrors.MoneyDecimalPlacesInvalid);
    [Fact] public void Create_RejectsMissingCurrency() => AssertError(() => Money.Create(1m, null!), DomainErrors.CurrencyCodeRequired);
}

public sealed class PaymentTermDaysTests
{
    [Fact] public void Create_AllowsZeroAndPositiveValues() { PaymentTermDays.Create(0).Value.Should().Be(0); PaymentTermDays.Create(30).ToString().Should().Be("30"); }
    [Fact] public void Create_RejectsNegativeValue() => AssertError(() => PaymentTermDays.Create(-1), DomainErrors.CustomerPaymentTermDaysInvalid);
}

public sealed class PercentageTests
{
    [Theory] [InlineData(0)] [InlineData(100)] [InlineData(12.5)] public void Create_AllowsValuesInRange(decimal value) => Percentage.Create(value).Value.Should().Be(value);
    [Theory] [InlineData(-0.01)] [InlineData(100.01)] public void Create_RejectsValuesOutsideRange(decimal value) => AssertError(() => Percentage.Create(value), DomainErrors.PercentageInvalid);
}

public sealed class PhoneNumberTests
{
    [Fact] public void Create_TrimsAndSupportsOptional() { PhoneNumber.Create(" +40700 ").Value.Should().Be("+40700"); PhoneNumber.CreateOptional(" ").Should().BeNull(); }
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => PhoneNumber.Create(null!), DomainErrors.CustomerPhoneRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => PhoneNumber.Create(new string('1', PhoneNumber.MaxLength + 1)), DomainErrors.CustomerPhoneTooLong);
}

public sealed class PostalCodeTests
{
    [Fact] public void Create_TrimsValue() => PostalCode.Create(" 010101 ").Value.Should().Be("010101");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => PostalCode.Create(" "), DomainErrors.CustomerPostalCodeRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => PostalCode.Create(new string('x', PostalCode.MaxLength + 1)), DomainErrors.CustomerPostalCodeTooLong);
}

public sealed class ProductCodeTests
{
    [Fact] public void Create_TrimsAndSupportsOptional() { ProductCode.Create(" PROD-1 ").Value.Should().Be("PROD-1"); ProductCode.CreateOptional(null).Should().BeNull(); }
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => ProductCode.Create(" "), DomainErrors.ProductCodeRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => ProductCode.Create(new string('x', ProductCode.MaxLength + 1)), DomainErrors.ProductCodeTooLong);
}

public sealed class ProductDescriptionTests
{
    [Fact] public void Create_TrimsAndSupportsOptional() { ProductDescription.Create(" Details ").Value.Should().Be("Details"); ProductDescription.CreateOptional(" ").Should().BeNull(); }
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => ProductDescription.Create(null!), DomainErrors.ProductDescriptionRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => ProductDescription.Create(new string('x', ProductDescription.MaxLength + 1)), DomainErrors.ProductDescriptionTooLong);
}

public sealed class ProductNameTests
{
    [Fact] public void Create_TrimsValue() => ProductName.Create(" Service ").Value.Should().Be("Service");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => ProductName.Create(" "), DomainErrors.ProductNameRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => ProductName.Create(new string('x', ProductName.MaxLength + 1)), DomainErrors.ProductNameTooLong);
}

public sealed class QuantityTests
{
    [Fact] public void Create_AllowsPositiveDecimalAndFormatsInvariantly() { Quantity.Create(2.5m).Value.Should().Be(2.5m); Quantity.Create(2.5m).ToString().Should().Be("2.5"); }
    [Theory] [InlineData(0)] [InlineData(-1)] public void Create_RejectsNonPositiveValue(decimal value) => AssertError(() => Quantity.Create(value), DomainErrors.QuantityInvalid);
}

public sealed class RegistrationNumberTests
{
    [Fact] public void Create_TrimsValue() => RegistrationNumber.Create(" REG-1 ").Value.Should().Be("REG-1");
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => RegistrationNumber.Create(" "), DomainErrors.CustomerRegistrationNumberRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => RegistrationNumber.Create(new string('x', RegistrationNumber.MaxLength + 1)), DomainErrors.CustomerRegistrationNumberTooLong);
}

public sealed class RequestHashTests
{
    [Fact] public void Create_NormalizesHexadecimalHash() => RequestHash.Create(new string('a', RequestHash.Sha256HexLength)).Value.Should().Be(new string('A', 64));
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => RequestHash.Create(" "), DomainErrors.RequestHashRequired);
    [Theory] [InlineData("abc")] [InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")] public void Create_RejectsInvalidHash(string value) => AssertError(() => RequestHash.Create(value), DomainErrors.RequestHashInvalid);
}

public sealed class SequenceNumberTests
{
    [Fact] public void Next_IncrementsAndFormatsWithSixDigits() { var next = SequenceNumber.Create(9).Next(); next.Value.Should().Be(10); next.ToString().Should().Be("000010"); }
    [Fact] public void Create_RejectsNonPositiveValue() => AssertError(() => SequenceNumber.Create(0), DomainErrors.SequenceNumberInvalid);
    [Fact] public void Next_RejectsOverflow() => AssertError(() => SequenceNumber.Create(long.MaxValue).Next(), DomainErrors.SequenceNumberExceededMaxValue);
}

public sealed class StateTests
{
    [Fact] public void Create_TrimsAndSupportsOptional() { State.Create(" State ").Value.Should().Be("State"); State.CreateOptional(" ").Should().BeNull(); }
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => State.Create(null!), DomainErrors.CustomerStateRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => State.Create(new string('x', State.MaxLength + 1)), DomainErrors.CustomerStateTooLong);
}

public sealed class TaxNumberTests
{
    [Fact] public void Create_TrimsAndSupportsOptional() { TaxNumber.Create(" RO123 ").Value.Should().Be("RO123"); TaxNumber.CreateOptional(null).Should().BeNull(); }
    [Fact] public void Create_RejectsMissingValue() => AssertError(() => TaxNumber.Create(" "), DomainErrors.CustomerTaxNumberRequired);
    [Fact] public void Create_RejectsOverlongValue() => AssertError(() => TaxNumber.Create(new string('x', TaxNumber.MaxLength + 1)), DomainErrors.CustomerTaxNumberTooLong);
}

internal static class ValueObjectTestAssertions
{
    internal static void AssertError(Action action, DomainError expectedError)
    {
        action.Should().Throw<DomainException>().WithDomainError(expectedError);
    }
}

