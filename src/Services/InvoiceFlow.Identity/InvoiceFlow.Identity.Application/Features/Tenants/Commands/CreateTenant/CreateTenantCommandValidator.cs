using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Slug)
            .NotEmpty();
    }
}
