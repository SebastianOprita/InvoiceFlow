using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetPlatformUsersQueryHandler(IPlatformUsersRepository usersRepository) : IRequestHandler<GetPlatformUsersQuery, Result<List<PlatformUserDto>>>
{
    public async Task<Result<List<PlatformUserDto>>> Handle(GetPlatformUsersQuery qry, CancellationToken cancellationToken)
    {
        var users = await usersRepository.FindAllUsersAsync();
        var usersDtos = users
            .Select(u => u.ToDto())
            .ToList();

        return Result<List<PlatformUserDto>>.Success(usersDtos);
    }
}
