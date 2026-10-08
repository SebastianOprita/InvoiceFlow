using InvoiceFlow.BuildingBlocks.Application;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class GetUserByIdQueryHandler(IUsersRepository usersRepository) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery qry, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetUserByIdAsync(qry.TenantId, qry.UserId, cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure(ApplicationErrors.UserNotFound);

        return Result<UserDto>.Success(user.ToDto());
    }
}
