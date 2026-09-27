using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetUsersQueryHandler(IUsersRepository usersRepository) : IRequestHandler<GetUsersQuery, Result<List<UserDto>>>
{
    public async Task<Result<List<UserDto>>> Handle(GetUsersQuery qry, CancellationToken cancellationToken)
    {
        var users = usersRepository.FindAllUsers(qry.TenantId)
            .Select(u => u.ToDto())
            .ToList();

        return Result<List<UserDto>>.Success(users);
    }
}
