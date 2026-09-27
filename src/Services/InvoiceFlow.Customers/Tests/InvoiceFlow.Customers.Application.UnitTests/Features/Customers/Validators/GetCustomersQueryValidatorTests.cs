using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Customers.Application.UnitTests.Features.Customers.Validators;

public sealed class GetCustomersQueryValidatorTests
{
    private readonly GetCustomersQueryValidator _validator = new();
    private readonly GetCustomersQuery GetCustomersQuery =
        new GetCustomersQuery(Guid.CreateVersion7());

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = GetCustomersQuery;

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_TenantId_Is_Empty()
    {
        var command = GetCustomersQuery
            with { TenantId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }
}
