using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers.Validators;

public sealed class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator = new();
    private readonly CreateCustomerCommand CreateCustomerCommand = 
        TestConstants.CreateCustomerCommand(Guid.CreateVersion7());

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = CreateCustomerCommand;

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        var command = TestConstants.CreateCustomerCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_CustomerCode_Is_Empty(string? customerCode)
    {
        var command = CreateCustomerCommand
            with { CustomerCode = customerCode! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerCode);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerCode_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerCode = new string('A', 39) 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Name_Is_Empty(string? name)
    {
        var command = TestConstants.CreateCustomerCommand(Guid.CreateVersion7(), name!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Max_Length()
    {
        var command = TestConstants.CreateCustomerCommand(Guid.CreateVersion7(), new string('A', 65));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Should_Have_Error_When_CurrencyCode_Is_Invalid(string? currencyCode)
    {
        var command = CreateCustomerCommand with 
        { 
            CurrencyCode = currencyCode! 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CurrencyCode);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Is_Null()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerContact = null! 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerContact);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Has_No_Email_Or_Phone()
    {
        var command = CreateCustomerCommand with
        {
            CustomerContact = new CustomerContact(null, null)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerContact");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Email_Is_Invalid()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerContact = CreateCustomerCommand.CustomerContact 
                with { Email = "invalid-email"} 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerContact.Email");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Email_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        {
            CustomerContact = CreateCustomerCommand.CustomerContact
                with { Email = $"{new string('a', 65)}@test.com" }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerContact.Email");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Phone_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        {
            CustomerContact = CreateCustomerCommand.CustomerContact
                with { Phone = new string('1', 16) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerContact.Phone");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerTaxDetails_Is_Null()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerTaxDetails = null! 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerTaxDetails);
    }

    [Fact]
    public void Should_Have_Error_When_TaxNumber_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        {
            CustomerTaxDetails = CreateCustomerCommand.CustomerTaxDetails 
                with { TaxNumber = new string('A', 65) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerTaxDetails.TaxNumber");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_RegistrationNumber_Is_Empty(string? registrationNumber)
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerTaxDetails = CreateCustomerCommand.CustomerTaxDetails 
                with { RegistrationNumber = registrationNumber! } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerTaxDetails.RegistrationNumber");
    }

    [Fact]
    public void Should_Have_Error_When_RegistrationNumber_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerTaxDetails = CreateCustomerCommand.CustomerTaxDetails 
                with { RegistrationNumber = new string('A', 16) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerTaxDetails.RegistrationNumber");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerAddress_Is_Null()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = null! 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerAddress);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_AddressLine1_Is_Empty(string? addressLine1)
    {
        var command = CreateCustomerCommand with 
        {
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { AddressLine1 = addressLine1! }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.AddressLine1");
    }

    [Fact]
    public void Should_Have_Error_When_AddressLine1_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { AddressLine1 = new string('A', 101) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.AddressLine1");
    }

    [Fact]
    public void Should_Have_Error_When_AddressLine2_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { AddressLine2 = new string('A', 101) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.AddressLine2");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_City_Is_Empty(string? city)
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { City = city! } 
        };
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.City");
    }

    [Fact]
    public void Should_Have_Error_When_City_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { City = new string('A', 51) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.City");
    }

    [Fact]
    public void Should_Have_Error_When_State_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { State = new string('A', 51) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.State");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Country_Is_Empty(string? country)
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { Country = country! } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.Country");
    }

    [Fact]
    public void Should_Have_Error_When_Country_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { Country = new string('A', 57) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.Country");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_PostalCode_Is_Empty(string? postalCode)
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { PostalCode = postalCode! } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.PostalCode");
    }

    [Fact]
    public void Should_Have_Error_When_PostalCode_Exceeds_Max_Length()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerAddress = CreateCustomerCommand.CustomerAddress 
                with { PostalCode = new string('A', 21) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.PostalCode");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerCreditPolicy_Is_Null()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerCreditPolicy = null! 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerCreditPolicy);
    }

    [Fact]
    public void Should_Have_Error_When_CreditLimit_Is_Negative()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerCreditPolicy = CreateCustomerCommand.CustomerCreditPolicy 
                 with { CreditLimit = -1 }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerCreditPolicy.CreditLimit");
    }

    [Fact]
    public void Should_Have_Error_When_PaymentTermDays_Is_Negative()
    {
        var command = CreateCustomerCommand with 
        { 
            CustomerCreditPolicy = CreateCustomerCommand.CustomerCreditPolicy 
                 with { PaymentTermDays = -1 } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerCreditPolicy.PaymentTermDays");
    }
}