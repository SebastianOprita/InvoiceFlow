using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetPlatformUserByIdQueryHandler(IPlatformUsersRepository usersRepository) : IRequestHandler<GetPlatformUserByIdQuery, Result<PlatformUserDto>>
{
    public async Task<Result<PlatformUserDto>> Handle(GetPlatformUserByIdQuery qry, CancellationToken ct)
    {
        var user = usersRepository.FindUserById(qry.UserId);

        if (user == null)
            return Result<PlatformUserDto>.Failure(ApplicationErrors.PlatformUserNotFound);

        return Result<PlatformUserDto>.Success(user.ToDto());
    }
}
