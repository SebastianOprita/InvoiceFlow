using FluentValidation;

namespace InvoiceFlow.Identity.Application;

public class GetPlatformUsersQueryValidator : AbstractValidator<GetPlatformUsersQuery>
{
    public GetPlatformUsersQueryValidator()
    {
    }
}
