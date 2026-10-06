using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetPlatformUsersQueryHandler(IPlatformUsersRepository platformUsersRepository) : IRequestHandler<GetPlatformUsersQuery, Result<List<PlatformUserDto>>>
{
    public async Task<Result<List<PlatformUserDto>>> Handle(GetPlatformUsersQuery qry, CancellationToken cancellationToken)
    {
        var platformUsers = await platformUsersRepository.FindAllUsersAsync();
        var platformUsersDtos = platformUsers
            .Select(u => u.ToDto())
            .ToList();

        return Result<List<PlatformUserDto>>.Success(platformUsersDtos);
    }
}
