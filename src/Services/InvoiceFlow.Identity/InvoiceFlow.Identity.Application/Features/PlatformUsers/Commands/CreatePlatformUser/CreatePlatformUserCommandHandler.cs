using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Domain;
using MediatR;

namespace InvoiceFlow.Identity.Application;

public class CreatePlatformUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPlatformUsersRepository usersRepository,
    IPasswordHasher passwordHasher,
    ISystemDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreatePlatformUserCommand, Result<PlatformUserDto>>
{
    public async Task<Result<PlatformUserDto>> Handle(CreatePlatformUserCommand cmd, CancellationToken cancellationToken)
    {
        var alreadyExists = await usersRepository.ExistsByEmailAsync(UserEmail.Create(cmd.Email), cancellationToken);

        if (alreadyExists)
            return Result<PlatformUserDto>.Failure(ApplicationErrors.UserEmailAlreadyExists);

        var passwordHash = passwordHasher.HashPassword(cmd.Password);
        var user = PlatformUser.Create(
            Guid.CreateVersion7(),
            UserEmail.Create(cmd.Email),
            PasswordHash.Create(passwordHash),
            FirstName.Create(cmd.FirstName),
            LastName.Create(cmd.LastName),
            dateTimeProvider.Now);
        usersRepository.AddUser(user);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (result.IsFailure)
            return Result<PlatformUserDto>.Failure(result);

        return Result<PlatformUserDto>.Success(user.ToDto());
    }
}
