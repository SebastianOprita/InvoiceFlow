using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class ChangePlatformUserPasswordCommandValidator : AbstractValidator<ChangePlatformUserPasswordCommand>
{
    public ChangePlatformUserPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty();
    }
}
