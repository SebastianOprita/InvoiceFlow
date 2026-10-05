using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class UpdatePlatformUserCommandValidator : AbstractValidator<UpdatePlatformUserCommand>
{
    public UpdatePlatformUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty();
    }
}
