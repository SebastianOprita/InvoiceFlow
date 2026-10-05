using FluentValidation;
using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public class ImpersonateUserCommandValidator : AbstractValidator<ImpersonateUserCommand>
{
    public ImpersonateUserCommandValidator()
    {
        RuleFor(x => x.ActorUserId)
            .NotEmpty();

        RuleFor(x => x.TargetUserTenantId)
            .NotEmpty();

        RuleFor(x => x.TargetUserEmail)
            .NotEmpty();

        RuleFor(x => x.DeviceInfo)
            .MaximumLength(DeviceInfo.MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceInfo));

        RuleFor(x => x.IpAddress)
            .MaximumLength(IpAddress.MaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress));
    }
}
