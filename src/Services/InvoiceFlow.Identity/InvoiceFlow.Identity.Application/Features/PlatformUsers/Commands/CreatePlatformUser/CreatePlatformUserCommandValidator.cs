using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class CreatePlatformUserCommandValidator : AbstractValidator<CreatePlatformUserCommand>
{
    public CreatePlatformUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);

        RuleFor(x => x.FirstName)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty();
    }
}
