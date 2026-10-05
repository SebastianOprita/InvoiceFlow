using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class DeactivatePlatformUserCommandValidator : AbstractValidator<DeactivatePlatformUserCommand>
{
    public DeactivatePlatformUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
