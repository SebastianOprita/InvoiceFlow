using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers.Validators;

public sealed class UpdateCustomerCommandValidatorTests
{
    private readonly UpdateCustomerCommandValidator _validator = new();
    private readonly UpdateCustomerCommand UpdateCustomerCommand = 
        TestConstants.UpdateCustomerCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = UpdateCustomerCommand;

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        var command = TestConstants.UpdateCustomerCommand(Guid.Empty, Guid.CreateVersion7());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        var command = TestConstants.UpdateCustomerCommand(Guid.CreateVersion7(), Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Max_Length()
    {
        var command = TestConstants.UpdateCustomerCommand(Guid.CreateVersion7(), Guid.CreateVersion7(), new string('A', 65));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Has_No_Email_Or_Phone()
    {
        var command = UpdateCustomerCommand with
        { 
            CustomerContact = new CustomerContact(null, null)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerContact");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Email_Is_Invalid()
    {
        var command = UpdateCustomerCommand with
        { 
            CustomerContact = UpdateCustomerCommand.CustomerContact! 
                with { Email = "invalid-email" } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerContact.Email");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Email_Exceeds_Max_Length()
    {
        var command = UpdateCustomerCommand with
        { 
            CustomerContact = UpdateCustomerCommand.CustomerContact! 
                with { Email = $"{new string('a', 65)}@test.com" } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerContact.Email");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerContact_Phone_Exceeds_Max_Length()
    {
        var command = UpdateCustomerCommand with
        { 
            CustomerContact = UpdateCustomerCommand.CustomerContact! 
                with { Phone = new string('1', 16) } 
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerContact.Phone");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_AddressLine1_Is_Empty(string addressLine1)
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress! 
                with { AddressLine1 = addressLine1! }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.AddressLine1");
    }

    [Fact]
    public void Should_Have_Error_When_AddressLine1_Exceeds_Max_Length()
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress! 
                with { AddressLine1 = new string('A', 101) }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.AddressLine1");
    }

    [Fact]
    public void Should_Have_Error_When_AddressLine2_Exceeds_Max_Length()
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress!
                with { AddressLine2 = new string('A', 101) }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.AddressLine2");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_City_Is_Empty(string city)
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress!
                with { City = city! }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.City");
    }

    [Fact]
    public void Should_Have_Error_When_City_Exceeds_Max_Length()
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress!
                with { City = new string('A', 51) }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.City");
    }

    [Fact]
    public void Should_Have_Error_When_State_Exceeds_Max_Length()
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress!
                with { State = new string('A', 51) }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.State");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Country_Is_Empty(string country)
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress!
                with { Country = country! }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.Country");
    }

    [Fact]
    public void Should_Have_Error_When_Country_Exceeds_Max_Length()
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress!
                with { Country = new string('A', 57) }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.Country");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_PostalCode_Is_Empty(string postalCode)
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress!
                with { PostalCode = postalCode! }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.PostalCode");
    }

    [Fact]
    public void Should_Have_Error_When_PostalCode_Exceeds_Max_Length()
    {
        var command = UpdateCustomerCommand with
        {
            CustomerAddress = UpdateCustomerCommand.CustomerAddress!
                with { PostalCode = new string('A', 21) }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerAddress.PostalCode");
    }

    [Fact]
    public void Should_Have_Error_When_CreditLimit_Is_Negative()
    {
        var command = UpdateCustomerCommand with
        { 
            CustomerCreditPolicy = UpdateCustomerCommand.CustomerCreditPolicy!
                with { CreditLimit = -1 }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerCreditPolicy.CreditLimit");
    }

    [Fact]
    public void Should_Have_Error_When_PaymentTermDays_Is_Negative()
    {
        var command = UpdateCustomerCommand with
        { 
            CustomerCreditPolicy = UpdateCustomerCommand.CustomerCreditPolicy!
                with { PaymentTermDays = -1 }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("CustomerCreditPolicy.PaymentTermDays");
    }
}
