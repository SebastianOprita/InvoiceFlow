using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetPlatformUserByIdQueryHandler(IPlatformUsersRepository usersRepository) : IRequestHandler<GetPlatformUserByIdQuery, Result<PlatformUserDto>>
{
    public async Task<Result<PlatformUserDto>> Handle(GetPlatformUserByIdQuery qry, CancellationToken cancellationToken)
    {
        var user = await usersRepository.FindUserByIdAsync(qry.UserId, cancellationToken);

        if (user == null)
            return Result<PlatformUserDto>.Failure(ApplicationErrors.UserUnauthorized);

        return Result<PlatformUserDto>.Success(user.ToDto());
    }
}
