using FluentValidation;

namespace InvoiceFlow.Identity.Application;

internal sealed class GetPlatformUsersQueryValidator : AbstractValidator<GetPlatformUsersQuery>
{
    public GetPlatformUsersQueryValidator()
    {
    }
}
