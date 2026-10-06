using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetPlatformUserByIdQueryHandler(IPlatformUsersRepository platformUsersRepository) : IRequestHandler<GetPlatformUserByIdQuery, Result<PlatformUserDto>>
{
    public async Task<Result<PlatformUserDto>> Handle(GetPlatformUserByIdQuery qry, CancellationToken cancellationToken)
    {
        var platformUser = await platformUsersRepository.FindUserByIdAsync(qry.UserId, cancellationToken);

        if (platformUser == null)
            return Result<PlatformUserDto>.Failure(ApplicationErrors.UserUnauthorized);

        return Result<PlatformUserDto>.Success(platformUser.ToDto());
    }
}
