using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetUserByIdQueryHandler(IUsersRepository usersRepository) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery qry, CancellationToken cancellationToken)
    {
        var user = await usersRepository.FindUserByIdAsync(qry.TenantId, qry.UserId);

        if (user == null)
            return Result<UserDto>.Failure(ApplicationErrors.UserNotFound);

        return Result<UserDto>.Success(user.ToDto());
    }
}
