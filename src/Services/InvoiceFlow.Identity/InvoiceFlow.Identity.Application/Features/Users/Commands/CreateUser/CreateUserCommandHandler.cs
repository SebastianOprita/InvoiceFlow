using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application.Features;

public class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IUsersRepository usersRepository,
    IPasswordHasher passwordHasher,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand cmd, CancellationToken cancellationToken)
    {
        var alreadyExists = await usersRepository.ExistsByEmailAsync(cmd.TenantId, UserEmail.Create(cmd.Email), cancellationToken);

        if (alreadyExists)
            return Result<UserDto>.Failure(ApplicationErrors.UserEmailAlreadyExists);

        var passwordHash = passwordHasher.HashPassword(cmd.Password);
        var user = User.Create(
            cmd.TenantId,
            Guid.CreateVersion7(),
            UserEmail.Create(cmd.Email),
            PasswordHash.Create(passwordHash),
            FirstName.Create(cmd.FirstName),
            LastName.Create(cmd.LastName),
            dateTimeProvider.Now);
        usersRepository.AddUser(user);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<UserDto>.Failure(result);

        return Result<UserDto>.Success(user.ToDto());
    }
}
