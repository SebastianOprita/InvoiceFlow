using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class DeactivatePlatformUserCommandValidator : AbstractValidator<DeactivatePlatformUserCommand>
{
    public DeactivatePlatformUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
