using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers.Validators;

public sealed class ActivateCustomerCommandValidatorTests
{
    private readonly ActivateCustomerCommandValidator _validator = new();
    private readonly ActivateCustomerCommand ActivateCustomerCommand =
        new ActivateCustomerCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = ActivateCustomerCommand;

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        var command = ActivateCustomerCommand
            with { TenantId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        var command = ActivateCustomerCommand
            with { CustomerId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }
}
