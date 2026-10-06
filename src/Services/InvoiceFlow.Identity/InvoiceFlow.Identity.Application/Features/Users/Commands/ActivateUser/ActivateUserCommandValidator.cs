using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
