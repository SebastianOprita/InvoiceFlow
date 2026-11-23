using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace InvoiceFlow.Payments.Application.UnitTests;

public sealed class PostPaymentCommandValidatorTests
{
    private readonly PostPaymentCommandValidator _sut = new();
    private static readonly PostPaymentCommand ValidCommand = new(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7());

    [Fact]
    public void Validate_AcceptsValidCommand()
        => _sut.TestValidate(ValidCommand).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_RequiresTenantId()
        => _sut.TestValidate(ValidCommand with { TenantId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.TenantId);

    [Fact]
    public void Validate_RequiresPaymentId()
        => _sut.TestValidate(ValidCommand with { PaymentId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.PaymentId);

    [Fact]
    public void Validate_RequiresPostedBy()
        => _sut.TestValidate(ValidCommand with { PostedBy = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.PostedBy);
}
